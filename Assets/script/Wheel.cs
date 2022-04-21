/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 07 2021 - 06:23
*/

using UnityEngine;
using WheelOfKnowledge.ExtensionMethods;

public sealed class Wheel : MonoBehaviour {
    public static event System.Action SpinStarted;
    public static event System.Action<CategoryObject> SpinFinished;

    [Header("Wheel")]
    public float Radius;
    public AnimationCurve SpinCurve;
    public float SpinDuration;
    public float SpinResultingAngleMultiplier;

    [Header("Segments")]
    public Color[] SegmentColors;
    public GameObject SegmentLabelPrefab;
    public float LabelOffsetFromSegmentCenter;
    public Shader SegmentShader;

    [Range(3, 30)]
    public int SegmentVertexCount;

    [Header("Stopper")]
    public GameObject Stopper;

    [Range(0, 360)]
    public int StopperPosition;

    private bool _wheelExists;

    private CategoryObject[] _categoryInstances;

    private Vector3 _vertexVector;
    private Color _vertexColor;
    private int _segmentColorIndex;
    private Material _segmentMaterial;
    private GameObject[] _segments;

    private GameObject _stopperInstance;

    private Vector3 _wheelScreenSpacePosition;
    private bool _isSpinning;
    private float _spinStartTime;
    private float _spinStartRotation;
    private float _spinEndRotation;

    private void Start() {
        // NOTE(sarper 01/15/21): Shader.Find id Unreliable, since the shader is not referenced anywhere it's stripped on build, so insteade of force-adding it via project settigns let's set it as field
        // _segmentMaterial = new Material(Shader.Find("Unlit/VertexColors"));
        _segmentMaterial = new Material(SegmentShader);
        _wheelScreenSpacePosition = Camera.main.WorldToScreenPoint(transform.position);
    }

    private void OnEnable() {
        CategoryBank.SelectedCategoryListPopulated += OnSelectedCategoryListPopulated;
        SwipeDetector.PlayerSwiped += OnPlayerSwiped;
        GameState.StateChanged += OnGameStateChanged;
        UIButtonEvents.QuitToMenuPressed += OnButtonQuitToMenuPressed;
    }

    private void OnDisable() {
        CategoryBank.SelectedCategoryListPopulated -= OnSelectedCategoryListPopulated;
        SwipeDetector.PlayerSwiped -= OnPlayerSwiped;
        GameState.StateChanged -= OnGameStateChanged;
        UIButtonEvents.QuitToMenuPressed -= OnButtonQuitToMenuPressed;
    }

    private void Update() {
        UpdateSpinning();
    }

    // NOTE(sarper 01/08/21): Since the wheel is created at runtime we need a way to visualize the size of the wheel and the position of the stopper etc. in the editor
    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, Radius);
        Gizmos.DrawWireSphere(transform.position + new Vector3(Mathf.Cos(StopperPosition * Mathf.Deg2Rad), Mathf.Sin(StopperPosition * Mathf.Deg2Rad), 0f) * Radius, 0.25f);
    }

    private void OnSelectedCategoryListPopulated(CategoryObject[] categories) {
        // NOTE(sarper 01/08/21): Shuffle segment colors so they don't appear the same every session
        SegmentColors.Shuffle();
        InstantiateCategories(categories);
        CreateWheel();
        CreateStopper();
    }

    /* NOTE(sarper 01/14/21): Calculate a start and an end rotation to linearly interpolate between
    Calculate two vectors, in screen space, going from wheel center to 1) Swipe start 2) Swipe end
    Get the signed angle between the two and multiply it with an arbitrary value just to increase the amount of rotation we will get as a result
    Linear interpolation start rotation is wheel's current rotation and it's end rotation is current + calculated angle * some value
    */
    private void OnPlayerSwiped(Vector3 swipeStart, Vector3 swipeEnd) {
        if(GameState.State != GameState.States.WaitingSpin) return;
        if(_isSpinning) return;

        Vector3 toSwipeStart = swipeStart - _wheelScreenSpacePosition;
        Vector3 toSwipeEnd = swipeEnd - _wheelScreenSpacePosition;
        float swipeAngle = Vector3.SignedAngle(toSwipeStart, toSwipeEnd, Vector3.forward);

        _spinEndRotation = transform.eulerAngles.z + swipeAngle * SpinResultingAngleMultiplier;

        _isSpinning = true;
        _spinStartTime = Time.time;
        _spinStartRotation = transform.eulerAngles.z;

        SpinStarted?.Invoke();
    }

    private void OnGameStateChanged(GameState.States state) {
        if(!_wheelExists) return;

        switch(state) {
            case GameState.States.MainMenu:
            case GameState.States.Question:
                ToggleWheelAndStopper(false);
                break;
            case GameState.States.WaitingSpin:
            case GameState.States.Spinning:
                ToggleWheelAndStopper(true);
                break;
        }
    }

    private void OnButtonQuitToMenuPressed() {
        for(int i = 0; i < _categoryInstances.Length; i++) {
            Destroy(_categoryInstances[i]);
            Destroy(_segments[i]);
        }

        Destroy(_stopperInstance);

        _isSpinning = false;
        _wheelExists = false;
        transform.rotation = Quaternion.identity;
    }

    private void InstantiateCategories(CategoryObject[] categories) {
        _categoryInstances = new CategoryObject[categories.Length];

        for(int i = 0; i < _categoryInstances.Length; i++) {
            _categoryInstances[i] = Instantiate(categories[i]);
        }
    }

    private void UpdateSpinning() {
        if(!_isSpinning) return;

        if(Time.time < _spinStartTime + SpinDuration) {
            float t = (Time.time - _spinStartTime) / SpinDuration;
            float spinCurAngle = Mathf.Lerp(_spinStartRotation, _spinEndRotation, SpinCurve.Evaluate(t));
            transform.rotation = Quaternion.Euler(0f, 0f, spinCurAngle);
        } else {
            _isSpinning = false;
            SpinFinished?.Invoke(_categoryInstances[FindCurrentlySelectedSegmentIndex()]);
        }
    }

    // NOTE(sarper 01/08/21): Finds the topmost wheel segment via dot product'ing between normalized segment position and Vector3.up. Highest dot is the current top segment.
    private int FindCurrentlySelectedSegmentIndex() {
        // NOTE(sarper 01/08/21): 0.1 less than the lowest value dot product between two normalized vectors can return
        float highest = -1.1f;
        int index = 0;

        for(int i = 0; i < _segments.Length; i++) {
            // NOTE(sarper 01/09/21): Wheel space positions already give us the vector from wheel center to desired position
            float dot = Vector3.Dot(_segments[i].transform.localPosition.normalized, _segments[i].transform.InverseTransformPoint(_stopperInstance.transform.position).normalized);

            if(dot > highest) {
                highest = dot;
                index = i;
            }
        }

        return index;
    }

    // NOTE(sarper 01/08/21): Creates a full wheel consisting of different segments. Number of segments match the number of question categories.
    private void CreateWheel() {
        _segments = new GameObject[_categoryInstances.Length];
        float segmentWidth = 2 * Mathf.PI / _categoryInstances.Length;

        for(int i = 0; i < _categoryInstances.Length; i++) {
            GameObject segment = CreateSegmentAsChild((i + 1) * segmentWidth, i * segmentWidth, _segmentMaterial, GetNextSegmentColor(), _categoryInstances[i].Name);
            _segments[i] = segment;
            CreateSegmentLabel(segment, _categoryInstances[i], LabelOffsetFromSegmentCenter);
        }

        _wheelExists = true;
    }

    /*
    NOTE(sarper 01/08/21):
    Creates a segment game object, makes it a child to this transform
    Adds the required MeshFilter and MeshRenderer components, also sets the material on the renderer
    Adds mesh filter and mesh renderer components to it
    Adds the '_segmentMaterial' to the mesh renderer so we can show the vertex colors on the segments
    Creates the circle segment mesh between the given angles(in radians) in CCW winding order using 'SegmentVertexCount' many vertices and assigns the vertex colors
    The created segment mesh and it's vertices are moved in a way so that it's pivot in the center
    */
    private GameObject CreateSegmentAsChild(float startAngle, float endAngle, Material material, Color vertexColor, string goName = "Segment") {
        GameObject segmentGo = new GameObject(goName);
        segmentGo.transform.SetParent(transform);
        segmentGo.transform.localPosition = Vector3.zero;

        MeshRenderer mrenderer = segmentGo.AddComponent<MeshRenderer>();
        mrenderer.material = material;

        MeshFilter mfilter = segmentGo.AddComponent<MeshFilter>();

        // NOTE(sarper 01/08/21): VERTICES
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[SegmentVertexCount];

        // NOTE(sarper 01/08/21): Moving the pivot to the center of the segment
        // NOTE(sarper 01/08/21): First find the heading of the segment and calculate a vector towards heading with a scale of half a radius
        float segmentAngle = (startAngle + endAngle) * 0.5f;
        Vector3 segmentDirection = new Vector3(Mathf.Cos(segmentAngle), Mathf.Sin(segmentAngle)).normalized;
        Vector3 pivotOffset = segmentDirection * Radius * 0.5f;

        // NOTE(sarper 01/08/21): Move the whole segment towards the calculated pivot offset
        segmentGo.transform.position += pivotOffset;

        // NOTE(sarper 01/08/21): Create the first vertex that always sits on the center of the transform, move it towards the inverse of the pivot offset
        vertices[0] = Vector3.zero - pivotOffset;

        // NOTE(sarper 01/08/21): Calculate the angle difference between each vertex that will sit on the segment arc, use the SegmentVertexCount to find that out
        int triangleCount = SegmentVertexCount - 2;
        float vertexInterval = (startAngle - endAngle) / triangleCount;

        // NOTE(sarper 01/08/21): Create the arc vertices each interval, offset all the vertices in the inverse direction of the pivot offset too, so finally the segment mesh has a pivot in the center as a result
        for(int i = 1; i < SegmentVertexCount; i++) {
            float angleOffset = (i - 1) * vertexInterval;
            _vertexVector.Set(Mathf.Cos(startAngle - angleOffset), Mathf.Sin(startAngle - angleOffset), 0f);
            vertices[i] = _vertexVector * Radius - pivotOffset;
        }

        mesh.vertices = vertices;

        // NOTE(sarper 01/08/21): INDICES
        int[] triangleIndices = new int[triangleCount * 3];

        for(int i = 0; i < triangleIndices.Length; i++) {
            // NOTE(sarper 01/08/21): We are converting the iterator value to mesh indices here so it goes like: 0 1 2 - 0 2 3 -  0 3 4...
            triangleIndices[i] = i % 3 == 0 ? 0 : i - (Mathf.FloorToInt(i / 3f) * 2);
        }

        mesh.triangles = triangleIndices;

        // NOTE(sarper 01/08/21): VERTEX COLORS
        Color[] vertexColors = new Color[mesh.vertices.Length];
        for(int i = 0; i < vertexColors.Length; i++) vertexColors[i] = vertexColor;

        mesh.colors = vertexColors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mfilter.mesh = mesh;

        return segmentGo;
    }

    // NOTE(sarper 01/09/21): Creates and initializes a label from label prefab for the given segment, makes it a child to the segment transform, rotates it to match to the rotation of the segment
    private void CreateSegmentLabel(GameObject segment, CategoryObject category, float positionOffsetFromCenter = 0) {
        GameObject label = Instantiate(SegmentLabelPrefab);
        Transform labelT = label.transform;

        labelT.parent = segment.transform;
        labelT.localPosition = Vector3.zero;
        labelT.localScale *= Radius;

        Vector3 toLabel = (labelT.position - transform.position).normalized;
        float angleToLabel = Vector3.SignedAngle(labelT.right, toLabel, Vector3.forward);
        labelT.rotation = Quaternion.AngleAxis(angleToLabel, Vector3.forward);

        labelT.localPosition += toLabel * positionOffsetFromCenter;

        label.GetComponent<TextMeshWrapper>().SetText(category.Name.ToUpper());
    }

    // NOTE(sarper 01/08/21): Gets the next color in the segment colors array, resets the next color index to 0 if we run out of colors
    private Color GetNextSegmentColor() {
        _vertexColor = SegmentColors[_segmentColorIndex];

        _segmentColorIndex++;
        if(_segmentColorIndex == SegmentColors.Length) _segmentColorIndex = 0;

        return _vertexColor;
    }

    // NOTE(sarper 01/09/21): Instantiate stopper, position it
    private void CreateStopper() {
        _stopperInstance = Instantiate(Stopper);
        _stopperInstance.transform.position = new Vector3(transform.position.x + Mathf.Cos(StopperPosition * Mathf.Deg2Rad) * Radius,
                                                          transform.position.y + Mathf.Sin(StopperPosition * Mathf.Deg2Rad) * Radius,
                                                          transform.position.z);
        _stopperInstance.transform.parent = transform.parent;
    }

    private void ToggleWheelAndStopper(bool toggle) {
        _stopperInstance.gameObject.SetActive(toggle);

        for(int i = 0; i < _segments.Length; i++) {
            _segments[i].SetActive(toggle);
        }
    }
}
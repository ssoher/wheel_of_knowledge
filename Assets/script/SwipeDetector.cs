/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 10 2021 - 08:56
*/

using UnityEngine;

public sealed class SwipeDetector : MonoBehaviour {
    // NOTE(sarper 01/14/21): Passing swipe start/end positions
    public static event System.Action<Vector3, Vector3> PlayerSwiped;

    public float MinAllowedSecondsForSwipe;
    public float MaxAllowedSecondsForSwipe;

    private bool _swipeStarted;
    private float _swipeStartTime;
    private float _swipeDuration;
    private Vector3 _swipeStartPosition;

    private void Update() {
        if(!_swipeStarted && Input.GetMouseButtonDown(0)) {
            _swipeStarted = true;
            _swipeStartTime = Time.time;
            _swipeStartPosition = Input.mousePosition;
        } else if(_swipeStarted && Input.GetMouseButtonUp(0)) {
            _swipeStarted = false;
            _swipeDuration = Time.time - _swipeStartTime;

            if(_swipeDuration > MinAllowedSecondsForSwipe && _swipeDuration < MaxAllowedSecondsForSwipe) {
                PlayerSwiped?.Invoke(_swipeStartPosition, Input.mousePosition);
            }
        }
    }
}
/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 14 2021 - 14:00
*/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuestionObject))]
public sealed class QuestionObjectEditor : Editor {
    private SerializedProperty _textProperty;
    private SerializedProperty _answersProperty;
    private int _correctAnswerIndex;
    private int _desiredSize;

    private void OnEnable() {
        _textProperty = serializedObject.FindProperty("Text");
        _answersProperty = serializedObject.FindProperty("Answers");

        _desiredSize = _answersProperty.arraySize;

        // NOTE(sarper 01/14/21): Get the correct answer from the object initially
        for(int i = 0; i < _answersProperty.arraySize; i++) {
            SerializedProperty isCorrect = _answersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("IsCorrect");
            if(isCorrect.boolValue) _correctAnswerIndex = i;
        }
    }

    // NOTE(sarper 01/14/21): A cleaner version of the default array inspector. Also toggle group functionality for selecting correct/wrong answers, something Unity editor api lacks
    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PrefixLabel("Question");
        _textProperty.stringValue = EditorGUILayout.TextField(_textProperty.stringValue);

        GUILayout.Space(20f);

        // NOTE(sarper 01/14/21): Drawing too many fields destroys editor performance, clamping is just a sanity check to make sure developer doesn't submit 100's due to a typo
        _desiredSize = EditorGUILayout.IntField("Answers", Mathf.Clamp(_desiredSize, 0, 25));

        // NOTE(sarper 01/14/21): Add or remove array elements based on the difference between desired size and current size
        if(_desiredSize != _answersProperty.arraySize) {
            int diff = _answersProperty.arraySize - _desiredSize;

            for(int i = 0; i < Mathf.RoundToInt(Mathf.Abs(diff)); i++) {
                if(Mathf.Sign(diff) > 0) {
                    _answersProperty.DeleteArrayElementAtIndex(_answersProperty.arraySize-1);
                } else {
                    _answersProperty.InsertArrayElementAtIndex(_answersProperty.arraySize);
                }
            }
        }

        GUILayout.Space(10f);

        // NOTE(sarper 01/14/21): Draw the Answer instances, mentioned toggle group functionality is implemented here
        for(int i = 0; i < _answersProperty.arraySize; i++) {
            SerializedProperty text = _answersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Text");
            SerializedProperty isCorrect = _answersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("IsCorrect");

            GUILayout.BeginHorizontal();

            text.stringValue = EditorGUILayout.TextField(text.stringValue);

            isCorrect.boolValue = _correctAnswerIndex == i;

            Color guiColor = GUI.color;
            GUI.color = isCorrect.boolValue ? Color.green : Color.red;

            if(GUILayout.Button(isCorrect.boolValue ? "Correct" : "Wrong")) {
                _correctAnswerIndex = i;
            }

            GUI.color = guiColor;

            GUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
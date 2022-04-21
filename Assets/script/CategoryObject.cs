/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 07 2021 - 05:15
*/

using UnityEngine;
using WheelOfKnowledge.ExtensionMethods;

[CreateAssetMenu(fileName = "Category", menuName = "Game/Category")]
public sealed class CategoryObject : ScriptableObject {
    public string Name;
    public QuestionObject[] Questions;

    private int _questionIndex;

    public QuestionObject GetNextQuestion() {
        QuestionObject q = Questions[_questionIndex];
        _questionIndex++;

        if(_questionIndex == Questions.Length) {
            Questions.Shuffle();
            _questionIndex = 0;
        }

        return q;
    }
}
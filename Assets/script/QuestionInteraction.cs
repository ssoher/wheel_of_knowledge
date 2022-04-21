/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 13 2021 - 06:59
*/

using UnityEngine;
using UnityEngine.UI;

public sealed class QuestionInteraction : MonoBehaviour {
    [Header("UI")]
    public Text QuestionText;
    public Text[] AnswerTexts;
    public UIAnswerColorAnimator[] AnswerColorAnimators;

    private QuestionObject _lastQuestionAsked;
    private bool _questionAnswered;

    private void OnEnable() {
        Wheel.SpinFinished += OnSpinFinished;
    }

    private void OnDisable() {
        Wheel.SpinFinished -= OnSpinFinished;
    }

    private void OnSpinFinished(CategoryObject category) {
        _lastQuestionAsked = category.GetNextQuestion();
        QuestionText.text = _lastQuestionAsked.Text;

        for(int i = 0; i < AnswerTexts.Length; i++) {
            AnswerTexts[i].text = _lastQuestionAsked.Answers[i].Text;
        }

        _questionAnswered = false;
    }

    // NOTE(sarper 01/13/21): Hooked up through the UnityEvents of the answer UI buttons
    public void OnAnswerButton(int index) {
        if(_questionAnswered == true) return;
        _questionAnswered = true;

        for(int i = 0; i < _lastQuestionAsked.Answers.Length; i++) {
            if(i == index && !_lastQuestionAsked.Answers[i].IsCorrect) {
                AnswerColorAnimators[i].Animate(3f, Color.red);
            } else if(_lastQuestionAsked.Answers[i].IsCorrect) {
                AnswerColorAnimators[i].Animate(3f, Color.green);
            }
        }
    }
}
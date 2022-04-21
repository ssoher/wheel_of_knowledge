/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 13 2021 - 16:22
*/

using UnityEngine;

public sealed class GameState : MonoBehaviour {
    public enum States { MainMenu, WaitingSpin, Spinning, Question }

    public static event System.Action<States> StateChanged;

    public static States State {
        private set;
        get;
    }

    private void OnEnable() {
        Wheel.SpinStarted += OnSpinStarted;
        Wheel.SpinFinished += OnSpinFinished;
        UIAnswerColorAnimator.ColorAnimationFinished += OnColorAnimationFinished;
        CategoryBank.GameInitiated += OnGameInitiated;
        UIButtonEvents.QuitToMenuPressed += OnQuitToMenuPressed;
    }

    private void OnDisable() {
        Wheel.SpinStarted -= OnSpinStarted;
        Wheel.SpinFinished -= OnSpinFinished;
        UIAnswerColorAnimator.ColorAnimationFinished -= OnColorAnimationFinished;
        CategoryBank.GameInitiated -= OnGameInitiated;
        UIButtonEvents.QuitToMenuPressed -= OnQuitToMenuPressed;
    }

    public void OnSpinStarted() {
        ChangeStateTo(States.Spinning);
    }

    public void OnSpinFinished(CategoryObject category) {
        ChangeStateTo(States.Question);
    }

    private void OnColorAnimationFinished() {
        ChangeStateTo(States.WaitingSpin);
    }

    public void OnGameInitiated(bool success) {
        if(!success) return;

        ChangeStateTo(States.WaitingSpin);
    }

    private void OnQuitToMenuPressed() {
        ChangeStateTo(States.MainMenu);
    }

    private void ChangeStateTo(States state) {
        if(State == state) {
            Debug.LogWarning($"Game state is already {state}");
            return;
        }

        State = state;
        StateChanged?.Invoke(state);
    }
}
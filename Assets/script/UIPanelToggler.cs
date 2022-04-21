/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 14 2021 - 12:07
*/

using UnityEngine;

public sealed class UIPanelToggler : MonoBehaviour {
    public RectTransform PanelMainMenu;
    public RectTransform PanelWheel;
    public RectTransform PanelQuestion;
    public RectTransform PanelNotEnoughCategories;

    private void OnEnable() {
        Wheel.SpinFinished += OnSpinFinished;
        GameState.StateChanged += OnGameStateChanged;
        CategoryBank.GameInitiated += OnGameInitiated;
    }

    private void OnDisable() {
        Wheel.SpinFinished -= OnSpinFinished;
        GameState.StateChanged -= OnGameStateChanged;
        CategoryBank.GameInitiated -= OnGameInitiated;
    }

    private void OnSpinFinished(CategoryObject category) {
        TogglePanel(PanelWheel, false);
        TogglePanel(PanelQuestion, true);
    }

    private void OnGameStateChanged(GameState.States state) {
        if(state != GameState.States.Question) {
            TogglePanel(PanelQuestion, false);

            if(state == GameState.States.WaitingSpin) TogglePanel(PanelWheel, true);
        }
    }

    private void OnGameInitiated(bool success) {
        if(!success) {
            TogglePanel(PanelMainMenu, false);
            TogglePanel(PanelNotEnoughCategories, true);
            return;
        }

        TogglePanel(PanelMainMenu, false);
        TogglePanel(PanelWheel, true);
    }

    private void TogglePanel(RectTransform panel, bool active) {
        panel.gameObject.SetActive(active);
    }
}
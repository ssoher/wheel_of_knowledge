/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 13 2021 - 15:10
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class CategoryBank : MonoBehaviour {
    public static event System.Action<CategoryObject[]> SelectedCategoryListPopulated;

    // NOTE(sarper 01/14/21): Bool parameter is to check for success, if player didn't select enough categories to play with, it returns false so we can show a popup to tell the player
    public static event System.Action<bool> GameInitiated;

    public CategoryObject[] Categories;

    [Header("UI")]
    public RectTransform CategoriesUIParent;
    public GameObject CategoryTogglePrefab;

    private Toggle[] _categoryToggleButtons;
    private List<CategoryObject> _selectedCategories;

    private void Awake() {
        CreateAndInitializeCategoryToggleButtons();
    }

    private void OnEnable() {
        UIButtonEvents.PlayPressed += OnPlayPressed;
    }

    private void OnDisable() {
        UIButtonEvents.PlayPressed -= OnPlayPressed;
    }

    public void OnPlayPressed() {
        if(NumberOfActiveCategoryToggles() < 2) {
            GameInitiated?.Invoke(false);
            return;
        }

        PopulateSelectedCategoriesList();
        GameInitiated?.Invoke(true);
    }

    private void CreateAndInitializeCategoryToggleButtons() {
        _categoryToggleButtons = new Toggle[Categories.Length];

        for(int i = 0; i < Categories.Length; i++) {
            GameObject cGo = Instantiate(CategoryTogglePrefab);
            cGo.transform.SetParent(CategoriesUIParent, false);
            Text t = cGo.GetComponentInChildren<Text>();
            t.text = Categories[i].Name;

            _categoryToggleButtons[i] = cGo.GetComponent<Toggle>();
        }

        _selectedCategories = new List<CategoryObject>();
    }

    private void PopulateSelectedCategoriesList() {
        _selectedCategories.Clear();

        for(int i = 0; i < _categoryToggleButtons.Length; i++) {
            if(_categoryToggleButtons[i].isOn) _selectedCategories.Add(Categories[i]);
        }

        SelectedCategoryListPopulated?.Invoke(_selectedCategories.ToArray());
    }

    private int NumberOfActiveCategoryToggles() {
        int num = 0;

        for(int i = 0; i < _categoryToggleButtons.Length; i++) {
            if(_categoryToggleButtons[i].isOn) num++;
        }

        return num;
    }
}
/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 14 2021 - 11:17
*/

using UnityEngine;

// NOTE(sarper 01/14/21): Methods in this class are invoked through UGUI Button UnityEvent's from their inspectors
public sealed class UIButtonEvents : MonoBehaviour {
    public static event System.Action QuitToMenuPressed;
    public static event System.Action PlayPressed;

    public void Play() {
        PlayPressed?.Invoke();
    }

    public void Settings() {
        // TODO(sarper 01/14/21): Implement
    }

    public void Leaderboards() {
        // TODO(sarper 01/14/21): Implement
    }

    public void QuitToMenu() {
        QuitToMenuPressed?.Invoke();
    }

    public void QuitGame() {
        UnityEngine.Application.Quit();
    }


}
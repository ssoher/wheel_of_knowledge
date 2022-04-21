/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 14 2021 - 09:00
*/

using UnityEngine;
using UnityEngine.UI;

public sealed class UIAnswerColorAnimator : MonoBehaviour {
    public static event System.Action ColorAnimationFinished;

    public Graphic Target;
    public AnimationCurve Curve;

    private bool _isPlaying;
    private Color _startColor;
    private Color _targetColor;
    private float _startTime;
    private float _duration;

    private void Start() {
        _startColor = Target.color;
    }

    private void OnDisable() {
        _isPlaying = false;
    }

    private void Update() {
        if(!_isPlaying) return;

        if(Time.time > _startTime + _duration) {
            _isPlaying = false;
            Target.color = _startColor;
            ColorAnimationFinished?.Invoke();
            return;
        }

        Target.color = Color.Lerp(_startColor, _targetColor, Curve.Evaluate((Time.time - _startTime) / _duration));
    }

    public void Animate(float duration, Color color) {
        _startTime = Time.time;
        _duration = duration;
        _targetColor = color;
        _isPlaying = true;
    }
}
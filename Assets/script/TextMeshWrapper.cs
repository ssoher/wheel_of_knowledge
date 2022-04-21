/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 09 2021 - 05:45
*/

using System.Text;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public sealed class TextMeshWrapper : MonoBehaviour {
    public int MinimumCharactersForWrap;

    private TextMesh _textMeshComponent;

    private void OnEnable() {
        _textMeshComponent = GetComponent<TextMesh>();
        Wrap();
    }

    public void SetText(string text) {
        _textMeshComponent.text = text;
        Wrap();
    }

    // NOTE(sarper 01/10/21): At each point the label text exceeds the given MinimumCharactersForWrap, add a line break at the next space character
    private void Wrap() {
        string[] words = _textMeshComponent.text.Split(' ');
        int elapsedSinceBreak = 0;

        StringBuilder stringBuilder = new StringBuilder();

        for(int i = 0; i < words.Length; i++) {
            stringBuilder.Append(words[i]);
            elapsedSinceBreak += words[i].Length;

            if(i != words.Length - 1) {
                if(elapsedSinceBreak > MinimumCharactersForWrap) {
                    stringBuilder.Append("\n");
                    elapsedSinceBreak = 0;
                } else {
                    stringBuilder.Append(" ");
                    elapsedSinceBreak++;
                }
            }

            _textMeshComponent.text = stringBuilder.ToString();
        }
    }
}
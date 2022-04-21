/*
Wheel of Knowledge
Sarper Soher - https://www.sarpersoher.com
Jan 07 2021 - 05:06
*/

using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "Game/Question")]
public sealed class QuestionObject : ScriptableObject {
    public string Text;
    public Answer[] Answers;
}
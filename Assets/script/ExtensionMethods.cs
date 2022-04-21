namespace WheelOfKnowledge.ExtensionMethods {
    using UnityEngine;

    public static class ExtensionMethods {
        public static void Shuffle<T>(this T[] array) {
            for(int i = 0; i < array.Length; i++) {
                int ri = Random.Range(0, array.Length);
                T temp = array[i];
                array[i] = array[ri];
                array[ri] = temp;
            }
        }
    }
}
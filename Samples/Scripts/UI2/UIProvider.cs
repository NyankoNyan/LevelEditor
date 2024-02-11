using System;

using UnityEngine;

namespace UI2
{
    public class UIProvider : MonoBehaviour
    {
        private static UIProvider s_instance;
        private readonly UIRoot _root = new();

        public static UIRoot Get()
        {
            if (!s_instance) {
                s_instance = FindObjectOfType<UIProvider>();
                if (!s_instance) {
                    throw new Exception($"Missing {typeof(UIProvider).Name} instance");
                }
            }
            return s_instance._root;
        }
    }
}
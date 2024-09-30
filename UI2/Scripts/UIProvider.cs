using System;

using UnityEngine;

namespace UI2
{
    public class UIProvider : MonoBehaviour
    {
        private static UIProvider s_instance;
        private UIRoot _root;

        void Awake()
        {
            _root = new(this);
        }

        public static UIRoot Get()
        {
            if (!s_instance) {
                s_instance = FindObjectOfType<UIProvider>();
                if (!s_instance) {
                    throw new Exception($"Missing {nameof(UIProvider)} instance");
                }
            }

            return s_instance._root;
        }
    }
}
using UnityEngine;

namespace UI
{
    public static class Vector2Extension
    {
        public static Vector2 ToVector2(this (float, float) other)
            => new(other.Item1, other.Item2);
    }
}
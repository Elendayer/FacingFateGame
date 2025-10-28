using UnityEngine;

namespace Utility
{
    public static class TransformUtility
    {
        public static void ZeroTransform(Transform t)
        {
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
        }
        public static void ZeroLocalTransform(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        public static void ZeroLocalRectTransform(RectTransform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
    }
}
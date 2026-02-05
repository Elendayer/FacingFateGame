using DG.Tweening;
using UnityEngine;

namespace dungeonduell
{
    public class TsCircleRotation : MonoBehaviour
    {
        public float rotationSpeed = 100f;

        public bool unscaledTime;

        private void Start()
        {
            var duration = 360f / rotationSpeed;
            transform.DORotate(new Vector3(0, 0, -360f), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(unscaledTime);
        }
    }
}
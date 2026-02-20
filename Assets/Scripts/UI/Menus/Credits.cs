using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dungeonduell
{
    public class Credits : MonoBehaviour
    {
        public RectTransform floatingText;
        public float speed;
        private bool up = true;

        // L�sst den Text automatsich ablaufen
        void Update()
        {
            if (floatingText.anchoredPosition.y <= -270)
            {
                up = true;
            }
            else if (floatingText.anchoredPosition.y >= 11)
            {
                up = false;
            }
            float speedScroll = speed * Time.deltaTime;
            floatingText.anchoredPosition += new Vector2(0f, up ? speedScroll : -speedScroll);
        }
    }
}
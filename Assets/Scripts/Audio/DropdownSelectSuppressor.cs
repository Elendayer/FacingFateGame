using UnityEngine;
using TMPro;

namespace facingfate
{
    public class DropdownSelectSuppressor : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private float suppressAfterValueChangeSeconds = 0.2f;

        private void Reset()
        {
            if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();
        }

        private void OnEnable()
        {
            if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();
            if (dropdown != null) dropdown.onValueChanged.AddListener(OnChanged);
        }

        private void OnDisable()
        {
            if (dropdown != null) dropdown.onValueChanged.RemoveListener(OnChanged);
        }

        private void OnChanged(int _)
        {
            //UiSelectSfx.SuppressSelectFor(suppressAfterValueChangeSeconds);
        }
    }
}

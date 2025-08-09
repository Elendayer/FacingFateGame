using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public Sprite IntentionImage(Intention intentions)
    {
        switch (intentions)
        {
            case Intention.None:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_0.png"); 
                case Intention.Attack:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_67.png"); 
                case Intention.Block:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_43.png");
                case Intention.Heal:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_20.png");
                case Intention.Buff:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_24.png");
                case Intention.Debuff:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_25.png");
                case Intention.BuffDebuff:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_24.png");
                case Intention.Other:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_26.png");
        }
        return null;
    }
}
using UnityEngine;

public class IntentionBar : MonoBehaviour
{


    public GameObject IntentionPrefab;

    public void addIntention(CardData data)
    {
        GameObject intentionObject = Instantiate(IntentionPrefab, transform);
        IntentionIcon intentionIcon = intentionObject.GetComponent<IntentionIcon>();

        intentionIcon.SetIcon(Intention.None, data.Power);
    }
    public void removeIntention()
    {

    }
}

public enum Intention
{
    None,
    Attack,
    Block,
    Heal,
    Buff,
    Debuff,
    BuffDebuff,
    Other,
}
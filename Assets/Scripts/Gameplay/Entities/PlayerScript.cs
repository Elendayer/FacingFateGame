using UnityEngine;

public class PlayerScript : EntityScript
{
    private void Awake()
    {
        base.entityAffiliation = EntityAffiliation.Player;
    }
    public override void StartUp()
    {
        base.StartUp();

        Debug.Log($"[PlayerScript] Setup complete for {name}");
        DeckManager.Instance.BuildDeckFromIDs(this);
    }
}
using UnityEngine;

public class PlayerManager : EntityManager
{
    // Singleton instance
    public static PlayerManager Instance { get; private set; }

    protected void Awake()
    {       
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persist between scenes
    }
}
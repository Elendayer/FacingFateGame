using UnityEngine;
using System.Collections;
using facingfate;


public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance { get; private set; }

    private bool listenersAdded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        AddListeners();
    }
    private void AddListeners()
    {
        if (listenersAdded) return;
        listenersAdded = true;

        GameEvents.OnCombatEnd += OnCombatEnd;
    }
    private void OnDestroy()
    {
        GameEvents.OnCombatEnd -= OnCombatEnd;
    }
    private void OnCombatEnd(bool playerWon)
    {
        // Stop all coroutines when combat ends to prevent lingering effects
        StopAllCoroutines();
    }
    public Coroutine StartCoroutineManaged(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    public void StopCoroutineManaged(Coroutine coroutine)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }
}
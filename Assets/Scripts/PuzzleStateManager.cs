using System.Collections.Generic;
using UnityEngine;

public class PuzzleStateManager : MonoBehaviour
{
    public static PuzzleStateManager Instance;

    private Dictionary<string, bool> boolStates = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(string key, bool value)
    {
        if (boolStates.ContainsKey(key))
            boolStates[key] = value;
        else
            boolStates.Add(key, value);

        Debug.Log("[PuzzleStateManager] " + key + " = " + value);
    }

    public bool GetState(string key)
    {
        if (boolStates.TryGetValue(key, out bool value))
            return value;

        return false;
    }
}
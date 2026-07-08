using System.Collections.Generic;
using UnityEngine;

public class ProgressionStore : MonoBehaviour
{
    public static ProgressionStore Instance;

    private Dictionary<ulong, ProgressionData> store = new Dictionary<ulong, ProgressionData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public ProgressionData GetData(ulong clientId)
    {
        if (!store.ContainsKey(clientId))
            store[clientId] = new ProgressionData();
        return store[clientId];
    }

    public void SaveData(ulong clientId, ProgressionData data)
    {
        store[clientId] = data;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public List<GoPManager.PoolType> pools = new List<GoPManager.PoolType>();
    public GoPManager poolManager;
    private void Awake()
    {
        poolManager = new GoPManager(gameObject);
        poolManager.Register(pools);
        foreach (var item in pools)
            poolManager.CreatePool(item.prefab);
    }
}

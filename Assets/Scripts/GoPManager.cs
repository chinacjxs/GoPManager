using System.Collections.Generic;
using UnityEngine;

public class GoPManager
{
    [System.Serializable]
    public class PoolType
    {
        public GameObject prefab;
        public int initialSize;
        public bool autoIncrease;
    }
    List<PoolType> pools = new List<PoolType>();
    GameObject attachedObject;
    Transform transform;
    Dictionary<GameObject, Queue<GameObject>> pooledObjects = new Dictionary<GameObject, Queue<GameObject>>();
    Dictionary<GameObject, List<GameObject>> spawnedObjects = new Dictionary<GameObject, List<GameObject>>();
    public GoPManager(GameObject attach)
    {
        attachedObject = attach;
        transform = attachedObject.transform;
    }
    public void Register(PoolType pool)
    {
        if (pool != null)
            if (!pools.Exists(p => { return p.prefab == pool.prefab; }))
                pools.Add(pool);
    }
    public void Register(List<PoolType> pools)
    {
        foreach (PoolType pool in pools)
            Register(pool);
    }
    public void CreatePool(GameObject prefab)
    {
        if (prefab != null)
        {
            PoolType startupPool = pools.Find(sp => { return sp.prefab == prefab; });
            if (startupPool != null)
            {
                if (!pooledObjects.ContainsKey(prefab))
                {
                    Queue<GameObject> poolObjects = new Queue<GameObject>();
                    for (int i = 0; i < startupPool.initialSize; i++)
                    {
                        GameObject go = GameObject.Instantiate(prefab, transform.position, transform.rotation, transform);
                        go.SetActive(false);
                        poolObjects.Enqueue(go);
                    }
                    pooledObjects.Add(prefab, poolObjects);
                }
            }
        }
    }
    public GameObject Spawn(GameObject prefab,Vector3 position,Quaternion rotation,Transform parent = null)
    {
        PoolType poolType = pools.Find(p => { return p.prefab == prefab; });
        if (prefab != null && poolType != null && pooledObjects.ContainsKey(prefab))
        {
            GameObject obj;
            if (pooledObjects.TryGetValue(prefab, out Queue<GameObject> poolObjects))
            {
                if (poolObjects.Count > 0)
                {
                    obj = poolObjects.Dequeue();
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    obj.transform.parent = parent;
                    obj.SetActive(true);
                }
                else
                {
                    if (poolType.autoIncrease)
                        obj = GameObject.Instantiate(prefab, position, rotation, parent);
                    else
                        obj = null;
                }
                if(obj != null)
                {
                    if (spawnedObjects.TryGetValue(prefab, out List<GameObject> spawnObjects))
                        spawnObjects.Add(obj);
                    else
                    {
                        spawnObjects = new List<GameObject> { obj };
                        spawnedObjects.Add(prefab, spawnObjects);
                    }
                }
                return obj;
            }
            else
                return null;          
        }
        else
            return null;
    }
    public void Recycle(GameObject prefab,GameObject obj)
    {
        if(prefab != null && obj != null)
        {
            if (spawnedObjects.TryGetValue(prefab, out List<GameObject> spawnObjects))
            {
                if (spawnObjects.Exists(o => { return o == obj; }))
                {
                    obj.transform.position = transform.position;
                    obj.transform.rotation = transform.rotation;
                    obj.transform.parent = transform;
                    obj.SetActive(false);
                    spawnObjects.Remove(obj);
                    pooledObjects[prefab].Enqueue(obj);
                }
            }
        }
    }
    public void Recycle(GameObject prefab)
    {
        if(prefab != null)
        {
            if(spawnedObjects.TryGetValue(prefab,out List<GameObject> spawnObjects))
                foreach (GameObject obj in spawnObjects)
                    Recycle(prefab, obj);
        }
    }
    public void RecycleAll()
    {
        foreach (var item in spawnedObjects)
            Recycle(item.Key);
    }
}

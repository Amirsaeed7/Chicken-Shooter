using System.Collections.Generic;
using UnityEngine;

public interface IPooledObject
{
    void OnSpawn();
    void OnDespawn();
}

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
    private readonly Dictionary<GameObject, Transform> roots = new();
    private readonly Dictionary<GameObject, GameObject> prefabByInstance = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
            var root = new GameObject($"{prefab.name}_Pool").transform;
            root.SetParent(transform);
            roots[prefab] = root;
        }

        GameObject item;
        while (queue.Count > 0)
        {
            item = queue.Dequeue();
            if (item != null)
            {
                item.transform.SetPositionAndRotation(position, rotation);
                item.SetActive(true);
                foreach (var pooled in item.GetComponents<IPooledObject>())
                    pooled.OnSpawn();
                prefabByInstance[item] = prefab;
                return item;
            }
        }

        item = Instantiate(prefab, position, rotation, roots[prefab]);
        item.SetActive(true);
        foreach (var pooled in item.GetComponents<IPooledObject>())
            pooled.OnSpawn();
        prefabByInstance[item] = prefab;
        return item;
    }

    public void Despawn(GameObject item)
    {
        if (item == null) return;
        foreach (var pooled in item.GetComponents<IPooledObject>())
            pooled.OnDespawn();
        item.SetActive(false);
        if (prefabByInstance.TryGetValue(item, out var prefab) &&
            pools.TryGetValue(prefab, out var queue))
            queue.Enqueue(item);
    }

    public void Register(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null) return;
        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
            var root = new GameObject($"{prefab.name}_Pool").transform;
            root.SetParent(transform);
            roots[prefab] = root;
        }
        queue.Enqueue(instance);
    }
}

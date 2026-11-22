public static class ObjectPoolExtensions
{
    public static void ReturnToPool(this UnityEngine.GameObject item)
    {
        if (ObjectPool.Instance != null) ObjectPool.Instance.Despawn(item);
    }
}

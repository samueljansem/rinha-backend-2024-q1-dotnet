using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.ObjectPool;

public interface IObjectPoolService
{
  T Rent<T>() where T : class, new();
  void Return<T>(T obj) where T : class;
  void RegisterPolicy<T>(IPooledObjectPolicy<T> policy, int size) where T : class;
}

public class ObjectPoolService : IObjectPoolService
{
  private readonly ObjectPoolProvider _objectPoolProvider;
  private readonly ConcurrentDictionary<Type, object> _pools = new ConcurrentDictionary<Type, object>();

  public ObjectPoolService(ObjectPoolProvider objectPoolProvider)
  {
    _objectPoolProvider = objectPoolProvider;
  }

  public T Rent<T>() where T : class, new()
  {
    var pool = (ObjectPool<T>)_pools.GetOrAdd(typeof(T), _ => _objectPoolProvider.Create(new DefaultPooledObjectPolicy<T>()));
    return pool.Get();
  }

  public void Return<T>(T obj) where T : class
  {
    if (_pools.TryGetValue(typeof(T), out var pool))
    {
      var typedPool = pool as ObjectPool<T>;
      typedPool?.Return(obj);
    }
  }

  public void RegisterPolicy<T>(IPooledObjectPolicy<T> policy, int size) where T : class
  {
    var pool = new CustomObjectPool<T>(policy, size);
    _pools.AddOrUpdate(typeof(T), pool, (_, __) => pool);
  }

  public void PopulatePool<T>(int size) where T : class, new()
  {

  }
}

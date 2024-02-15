using System;
using System.Threading;
using Microsoft.Extensions.ObjectPool;

public class CustomObjectPool<T> : DefaultObjectPool<T> where T : class
{
  private int _usedObjectsCount = 0;

  public CustomObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained) : base(policy, maximumRetained) { }

  public override T Get()
  {
    var obj = base.Get();
    Interlocked.Increment(ref _usedObjectsCount);
    PrintPoolStatus();
    return obj;
  }

  public override void Return(T obj)
  {
    base.Return(obj);
    Interlocked.Decrement(ref _usedObjectsCount);
    PrintPoolStatus();
  }

  private void PrintPoolStatus()
  {
    var objectType = typeof(T).Name;
    Console.WriteLine($"[{objectType}] Used Objects: {_usedObjectsCount}");
  }
}


using System.Collections.Generic;

public interface IPooledObjectTracker
{
  void TrackObject<T>(T obj) where T : class;
  void ReturnAllTrackedObjects();
}

public class PooledObjectTracker : IPooledObjectTracker
{
  private readonly IObjectPoolService _pool;
  private readonly List<object> _tracked = new();

  public PooledObjectTracker(IObjectPoolService pool)
  {
    _pool = pool;
  }

  public void ReturnAllTrackedObjects()
  {
    foreach (var obj in _tracked)
    {
      _pool.Return(obj);
    }

    _tracked.Clear();
  }

  public void TrackObject<T>(T obj) where T : class
  {
    _tracked.Add(obj);
  }
}
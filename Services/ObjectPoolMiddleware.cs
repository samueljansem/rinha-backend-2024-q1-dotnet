using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class ObjectPoolMiddleware
{
  private readonly RequestDelegate _next;

  public ObjectPoolMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context, IPooledObjectTracker tracker)
  {
    await _next(context);
    tracker.ReturnAllTrackedObjects();
  }
}
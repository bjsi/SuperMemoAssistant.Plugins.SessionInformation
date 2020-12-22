using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace SuperMemoAssistant.Plugins.SessionInformation.Helpers
{
  public static class RxEx
  {
    /// <summary>
    /// [1, 2, 3, 4] => [[1, 2], [2, 3], [3, 4], [4]]
    /// Note: last list will be a list containing one element
    /// </summary>
    public static IObservable<IList<T>> ToPairwiseBuffer<T>(this IObservable<T> observable)
    {
      return observable.Buffer(2, 1);
    }

    /// <summary>
    /// Allows you to get information about what the reactive query is doing with console logging.
    /// </summary>
    /// <param name="source">Input observable</param>
    /// <param name="opName">Human-friendly name of the operation</param>
    public static IObservable<T> Spy<T>(this IObservable<T> source, string opName = null)
    {
      return Spy(source, opName, Console.WriteLine);
    }

    /// <summary>
    /// Allows you to get information about what the reactive query is doing with logging.
    /// </summary>
    /// <param name="source">Input observable</param>
    /// <param name="opName">Human-friendly name of the operation</param>
    /// <param name="logger">Logger</param>
    public static IObservable<T> Spy<T>(this IObservable<T> source, string opName,
                                                                  Action<string> logger)
    {
      opName = opName ?? "IObservable";
      logger($"{opName}: Observable obtained on Thread: {Thread.CurrentThread.ManagedThreadId}");

      var count = 0;
      return Observable.Create<T>(obs =>
      {
        logger($"{opName}: Subscribed to on Thread: {Thread.CurrentThread.ManagedThreadId}");
        try
        {
          var subscription = source
              .Do(x => logger($"{opName}: OnNext({x}) on Thread: {Thread.CurrentThread.ManagedThreadId}"),
                  ex => logger($"{opName}: OnError({ex}) on Thread: {Thread.CurrentThread.ManagedThreadId}"),
                  () => logger($"{opName}: OnCompleted() on Thread: {Thread.CurrentThread.ManagedThreadId}")
              )
              .Subscribe(t =>
              {
                try
                {
                  obs.OnNext(t);
                }
                catch (Exception ex)
                {
                  logger($"{opName}: Downstream exception ({ex}) on Thread: {Thread.CurrentThread.ManagedThreadId}");
                  throw;
                }
              }, obs.OnError, obs.OnCompleted);

          return new CompositeDisposable(
                  Disposable.Create(() => logger($"{opName}: Dispose (Unsubscribe or Observable finished) on Thread: {Thread.CurrentThread.ManagedThreadId}")),
                  subscription,
                  Disposable.Create(() => Interlocked.Decrement(ref count)),
                  Disposable.Create(() => logger($"{opName}: Dispose (Unsubscribe or Observable finished) completed, {count} subscriptions"))
              );
        }
        finally
        {
          Interlocked.Increment(ref count);
          logger($"{opName}: Subscription completed, {count} subscriptions.");
        }
      });
    }
  }
}

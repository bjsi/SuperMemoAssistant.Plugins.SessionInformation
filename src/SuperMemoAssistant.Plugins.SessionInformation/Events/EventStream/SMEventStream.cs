using SuperMemoAssistant.Plugins.SessionInformation.Helpers;
using SuperMemoAssistant.Plugins.SessionInformation.Interop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace SuperMemoAssistant.Plugins.SessionInformation.Events.EventStream
{
  public static class SMEventStream
  {

    public static IObservable<SuperMemoEvent> CreateMergedStreams(params IObservable<SuperMemoEvent>[] streams)
    {
      if (streams.IsNullOrEmpty())
        throw new ArgumentException("Input streams are null or empty");

      // TODO: Document
      return streams
        .Aggregate((fst, snd) => fst.Merge(snd))
        .Publish()
        .RefCount();
    }

    /// <summary>
    /// Start observing the merged event stream
    /// </summary>
    public static IObservable<IList<SuperMemoEvent>> CreateEventStream(IObservable<SuperMemoEvent> mergedStream, double afkTime)
    {
      return mergedStream
         .ToPairwiseBuffer()
         .Select(arr => arr[0])
         .Window(mergedStream
             .ToPairwiseBuffer()
             .Where(pair =>
                  (pair[0].ElementId != pair[1].ElementId) ||
                  (pair[1].Timestamp - pair[0].Timestamp).TotalSeconds > afkTime)
         )
         .SelectMany(x => x.ToList())
         .Where(x => x.Count >= 2);  // Require minimum of two events to create a snapshot
         // Subscribe On / Observe On
         .Subscribe(x =>
         {
           var events = x.OrderBy(x => x.Timestamp).ToList();
           var snapshot = new SummarySnapshot(events);
           _sessionInformationService.SummarySnapshots.Add(snapshot);
         });
    }

    /// <summary>
    /// Creates an observable from the MouseMove event
    /// </summary>
    /// <returns></returns>
    private IObservable<SuperMemoEvent> CreateBufferedMouseObservable()
    {
      return Observable
        .FromEventPattern<MouseHookEventArgs>(
          h => MouseHook.Move += h,
          h => MouseHook.Move -= h
        )
        // Buffered because this produces a huge number of events
        .Buffer(TimeSpan.FromSeconds(1))
        .Where(x => x.Count > 0)
        .ManySelect(_ => CreateSuperMemoEvent(EventOrigin.Mouse, false));
    }

    /// <summary>
    /// Creates an observable from the HotKey key pressed event
    /// TODO: Change to KeyboardPressedEvent when available
    /// </summary>
    /// <returns></returns>
    private IObservable<SuperMemoEvent> CreateHotKeyObservable()
    {
      return Observable
        .FromEvent<HotKey>(
          h => Svc.KeyboardHotKey.MainCallback += h,
          h => Svc.KeyboardHotKey.MainCallback -= h
        )
        .Select(_ => CreateSuperMemoEvent(EventOrigin.Keyboard, false));
    }

    /// <summary>
    /// Creates an observable from the HtmlDoc KeyDown event
    /// This is used as a proxy for an element changed event.
    /// TODO: Change to ElementChanged or to KeyPressedEvent
    /// </summary>
    /// <returns></returns>
    private IObservable<SuperMemoEvent> CreateElementEditObservable()
    {
      return Observable
        .FromEventPattern<IHTMLEventObj>(
          h => HtmlDocKeyPressEvent.OnEvent += h,
          h => HtmlDocKeyPressEvent.OnEvent -= h
        )
        // TODO: Describe the effect of throttle
        .Throttle(TimeSpan.FromSeconds(0.45))
        .Select(_ => CreateSuperMemoEvent(EventOrigin.EditedElement, true));
    }

    /// <summary>
    /// Creates an observable from the DisplayedElementChanged event
    /// TODO: Only using e.NewElement
    /// </summary>
    /// <returns></returns>
    private IObservable<SuperMemoEvent> CreateDisplayedElementObservable()
    {
      return Observable
        .FromEventPattern<SMDisplayedElementChangedEventArgs>(
          h => DisplayedElementChanged += h,
          h => DisplayedElementChanged -= h
        )
        .Select(_ => CreateSuperMemoEvent(EventOrigin.DisplayedElementChanged, true));
    }
  }
}

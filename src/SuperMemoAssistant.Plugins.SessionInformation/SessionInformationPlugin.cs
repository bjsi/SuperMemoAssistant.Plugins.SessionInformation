using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using mshtml;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.Sys.IO.Devices;
using SuperMemoAssistant.Sys.Remoting;
using SuperMemoAssistant.Plugins.SessionInformation.MouseHook;
using System.Linq;
using Anotar.Serilog;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Plugins.SessionInformation.Helpers;
using static SuperMemoAssistant.Plugins.SessionInformation.Helpers.HtmlEventEx;
using System.Threading.Tasks;
using System.Threading;
using SuperMemoAssistant.Plugins.SessionInformation.Interop.Models;
using SuperMemoAssistant.Plugins.SessionInformation.Events.Html;
using SuperMemoAssistant.Plugins.SessionInformation.Events.Mouse;

#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   6/3/2020 3:03:53 PM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.SessionInformation
{
  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class SessionInformationPlugin : SentrySMAPluginBase<SessionInformationPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public SessionInformationPlugin() : base("Enter your Sentry.io api key (strongly recommended)") { }

    /// <summary>Plugin config class</summary>
    public SessionInformationCfg Config { get; set; }

    /// <summary>Raises events on html control keypresses</summary>
    private HtmlEventObject HtmlDocKeyPressEvent { get; set; }

    /// <summary>Mouse hook with event that fires when the user moves the mouse</summary>
    private MouseMoveHook MouseHook { get; } = new MouseMoveHook();

    private DateTime LastUpdatedContent { get; set; } = DateTime.MinValue;

    private IObservable<SuperMemoEvent> EventStream { get; set; }

    private bool Observing { get; set; } = false;

    /// <summary>Service published for other plugins</summary>
    private SessionInformationService _sessionInformationService { get; set; }

    // TODO: Is this a hack - daisy chaining the ActionProxy event to a normal event
    private event EventHandler<SMDisplayedElementChangedEventArgs> DisplayedElementChanged;
    #endregion

    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "SessionInformation";

    /// <inheritdoc />
    public override bool HasSettings => true;

    #endregion

    private void LoadConfig()
    {
      Config = Svc.Configuration.Load<SessionInformationCfg>() ?? new SessionInformationCfg();
    }

    /// <inheritdoc />
    public override void ShowSettings()
    {
      ConfigurationWindow.ShowAndActivate(HotKeyManager.Instance, Config);
    }

    #region Methods Impl

    // TODO: 
    protected override void Dispose(bool disposing)
    {
      MouseHook?.Dispose();
      base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override void PluginInit()
    {

      LoadConfig();

      _sessionInformationService = new SessionInformationService();

      // Create and Publish the service
      PublishService<ISessionInformationService, SessionInformationService>(_sessionInformationService);

      // Information Collection begins from the first element changed event
      Svc.SM.UI.ElementWdw.OnElementChanged += 
        new ActionProxy<SMDisplayedElementChangedEventArgs>(OnDisplayedElementChanged);

    }


    private void OnDisplayedElementChanged(SMDisplayedElementChangedEventArgs e)
    {
      // TODO: Fix this
      DisplayedElementChanged?.Invoke(null, e);

      SubscribeToKeyDownEvents();

      // Create reactivex event streams
      var MouseMoved = CreateBufferedMouseObservable();
      var HotKeyPressed = CreateHotKeyObservable();
      var ElementEdited = CreateElementEditObservable();
      var ElementChanged = CreateDisplayedElementObservable();

      if (!Observing)
        BeginObserving();
    }

    /// <summary>
    /// Add keydown events to all html documents on the current element.
    /// </summary>
    [LogToErrorOnException]
    private void SubscribeToKeyDownEvents()
    {

      var ctrlGrp = Svc.SM.UI.ElementWdw.ControlGroup;
      var htmlCtrls = ctrlGrp?.GetAllHtmlDocuments();
      if (htmlCtrls.IsNullOrEmpty())
        return;

      foreach (var htmlCtrl in htmlCtrls.Values)
      {
        if (!(htmlCtrl.body is IHTMLElement2 body))
          continue;
        body.SubscribeTo(HtmlEvents.onkeypress, HtmlDocKeyPressEvent);
      }
    }


    private SuperMemoEvent CreateSuperMemoEvent(EventOrigin Origin, bool IncludeContent)
    {
      var el = Svc.SM.UI.ElementWdw.CurrentElement;
      string Content = string.Empty;
      if ((DateTime.Now - LastUpdatedContent).TotalSeconds > 1 || IncludeContent)
        Content = ContentFuncs.GetCurrentElContent();
      return new SuperMemoEvent(el.Id, Origin, Content);
    }



    #endregion


    #region Methods

    #endregion
  }
}

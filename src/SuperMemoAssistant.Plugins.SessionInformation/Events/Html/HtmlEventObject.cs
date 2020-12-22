using mshtml;
using System;
using System.Runtime.InteropServices;

namespace SuperMemoAssistant.Plugins.SessionInformation.Events.Html
{
  [ComVisible(true)]
  [ClassInterface(ClassInterfaceType.AutoDispatch)]
  public class HtmlEventObject
  {
    public event EventHandler<IHTMLEventObj> OnEvent;

    [DispId(0)]
    public void handler(IHTMLEventObj e) => OnEvent?.Invoke(this, e);
  }
}

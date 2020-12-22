using mshtml;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.SessionInformation.Events.Html;
using System;
using System.Runtime.Remoting;

namespace SuperMemoAssistant.Plugins.SessionInformation.Helpers
{
  public static class IHtmlElementEx
  {
    public static HtmlEventObject SubscribeTo(this IHTMLElement2 element, HtmlEvents eventType, HtmlEventObject handlerObj)
    {
      try
      {
        element?.attachEvent(eventType.Name(), handlerObj);
      }
      catch (RemotingException) { }
      catch (UnauthorizedAccessException) { }

      return handlerObj;
    }

    public static void UnsubscribeFrom(this IHTMLElement2 element, HtmlEvents eventType, HtmlEventObject eventObj)
    {
      try
      {
        element?.detachEvent(eventType.Name(), eventObj);
      }
      catch (RemotingException) { }
      catch (UnauthorizedAccessException) { }
    }
  }
}

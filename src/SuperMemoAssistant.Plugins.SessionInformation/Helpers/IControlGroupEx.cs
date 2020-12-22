using mshtml;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SuperMemoAssistant.Plugins.SessionInformation.Helpers
{
  public static class IControlGroupEx
  {
    public static Dictionary<int, IHTMLDocument2> GetAllHtmlDocuments(this IControlGroup grp)
    {

      var ret = new Dictionary<int, IHTMLDocument2>();

      try
      {

        if (grp == null || grp.IsDisposed)
          return ret;

        for (int i = 0; i < grp.Count; i++)
        {
          if (!(grp[i] is IControlHtml htmlCtrl))
            continue;

          ret[i] = htmlCtrl.GetDocument();
        }
      }
      catch (UnauthorizedAccessException) { }
      catch (COMException) { }

      return ret;
    }
  }
}

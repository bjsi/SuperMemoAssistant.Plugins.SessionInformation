using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Plugins.SessionInformation.Helpers
{
  public static class IEnumerableEx
  {
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> x)
    {
      return x == null || !x.Any();
    }
  }
}

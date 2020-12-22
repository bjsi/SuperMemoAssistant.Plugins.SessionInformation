using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginManager.Interop.Sys;
using SuperMemoAssistant.Plugins.SessionInformation.Interop;
using SuperMemoAssistant.Plugins.SessionInformation.Interop.Models;

namespace SuperMemoAssistant.Plugins.SessionInformation
{
  internal class SessionInformationService : PerpetualMarshalByRefObject, ISessionInformationService
  {
    public List<SummarySnapshot> SummarySnapshots { get; set; }

    public SessionInformationService()
    {
      SummarySnapshots = new List<SummarySnapshot>();
    }
  }
}

using Forge.Forms.Annotations;
using mshtml;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;
using System.ComponentModel;

namespace SuperMemoAssistant.Plugins.SessionInformation
{
  [Form(Mode = DefaultFields.None)]
  [Title("Session Information Settings",
         IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
                "Cancel",
                IsCancel = true)]
  [DialogAction("save",
                "Save",
                IsDefault = true,
                Validates = true)]
  public class SessionInformationCfg : CfgBase<SessionInformationCfg>, INotifyPropertyChangedEx 
  {
    [Title("Session Information Plugin")]

    [Heading("By Jamesb | Experimental Learning")]

    [Heading("Features:")]
    [Text(@"- Collects data about your current learning session.
- Integrates with other plugins to save data locally or send it to time tracking applications.
- Publishes itself as a service to provide data to other plugins.")]

    [Field(Name = "AFK time (seconds)")]
    public int AFKTime { get; set; } = 30;

    [JsonIgnore]
    public bool IsChanged { get; set; }

    public override string ToString()
    {
      return "Session Information";
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}

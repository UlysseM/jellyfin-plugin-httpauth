using System.Net.Http.Headers;
using MediaBrowser.Model.Plugins;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.HttpAuth.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {

        public PluginConfiguration()
        {
            EnablePlugin = false;
            EnableSafetyBreaker = true;
            BreakerTripped = false;
            UserHeader = "X-Forwarded-User";
            NewUsersAreAdmin= false;
        }

        public bool EnablePlugin { get; set; }
        public bool EnableSafetyBreaker { get; set; }
        public bool BreakerTripped { get; set; }
        public string UserHeader { get; set; }
        public bool NewUsersAreAdmin{ get; set; }

    }
}

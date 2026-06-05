using System.Net.Http.Headers;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.HttpAuth.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {

        public PluginConfiguration()
        {
            EnablePlugin = false;
            UserHeader = "X-Forwarded-User";
            NewUsersAreAdmin= false;
        }

        public bool EnablePlugin { get; set; }
        public string UserHeader { get; set; }
        public bool NewUsersAreAdmin{ get; set; }

    }
}

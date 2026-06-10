using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations.Entities.Libraries;
using Jellyfin.Plugin.HttpAuth.Configuration;
using MediaBrowser.Common;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HttpAuth
{
    // The purpose of this file is to serve the file InjectAuth/script.js, from the Configuration folder.
    [ApiController]
    [Route("[controller]")]
    public class InjectAuthController : ControllerBase
    {
        static public string BlockToInject => "<script src=\"/InjectAuth/script.js\" defer></script>";
        private static PluginConfiguration Config { get { return Plugin.Instance.Configuration; } }

        [HttpGet("script.js")]
        public ActionResult Login()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.injectAuth.js", GetType().Namespace));
            if (stream == null)
            {
                return NotFound();
            }
            return File(stream, "text/javascript");
        }

        [HttpGet("auth_data")]
        public IActionResult IsEnabled()
        {
            bool enabled = Config.EnablePlugin;
            string username = HttpAuthProvider.GetUsernameFromRequest(Request) ?? "";
            if (enabled && username == "" && Config.EnableSafetyBreaker)
            {
                // The safety breaker was enabled, and we didn't notice any connection coming in, so we'll trip it.
                Plugin.Instance.TripSafetyMechanism();
                enabled = false;
            }
            return Ok(new
            {
                Enabled = enabled,
                Username = username,
                Config.BreakerTripped,
            });
        }
    }

}

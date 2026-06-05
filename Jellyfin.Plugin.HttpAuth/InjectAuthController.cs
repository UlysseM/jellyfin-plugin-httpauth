using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Session;
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

        public InjectAuthController() {}

        [HttpGet("script.js")]
        public ActionResult Login()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.injectAuth.js", GetType().Namespace));
            return File(stream, "text/javascript");
        }
    }

}

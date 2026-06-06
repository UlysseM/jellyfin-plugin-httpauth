using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Jellyfin.Plugin.HttpAuth.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HttpAuth
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly ILogger<Plugin> _logger;
        private readonly string _indexPath;

        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer, ILogger<Plugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            // Now, let's also inject the JS in the index.html.

            _logger = logger;
            _indexPath = Path.Combine(applicationPaths.WebPath, "index.html");
            InjectToIndex();
        }

        public void TripSafetyMechanism()
        {
            _logger.LogError("A request came to the server without having the header {UserHeader} set. Since the SafetyBreaker is enabled, we'll trip it by disabling the plugin.", Configuration.UserHeader);
            Configuration.EnablePlugin = false;
            Configuration.BreakerTripped = true;
            UpdateConfiguration(Configuration);
        }

        // Inspired by n00bcodr's Jellyfin-JavaScript-Injector.
        void InjectToIndex()
        {
            if (!File.Exists(_indexPath))
            {
                _logger.LogError("Could not find index.html at path: {Path}", _indexPath);
                return;
            }
            try
            {
                var content = File.ReadAllText(_indexPath);
                if (content.Contains(InjectAuthController.BlockToInject, StringComparison.Ordinal))
                {
                    _logger.LogInformation("HttpAuth javascript already injected.");
                    return;
                }
                var closingBodyTag = "</body>";
                if (content.Contains(closingBodyTag, StringComparison.Ordinal))
                {
                    content = content.Replace(closingBodyTag, $"{InjectAuthController.BlockToInject}{closingBodyTag}", StringComparison.Ordinal);
                    File.WriteAllText(_indexPath, content);
                    _logger.LogInformation("Successfully injected the JavaScriptInjector script block.");
                }
                else
                {
                    _logger.LogWarning("Could not find </body> tag in index.html. Scripts not injected.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while trying to modify script from index.html during install.");
            }
        }
        public override void OnUninstalling()
        {
            try
            {
                if (!File.Exists(_indexPath))
                {
                    _logger.LogError("Could not find index.html at path: {Path}", _indexPath);
                    return;
                }

                var content = File.ReadAllText(_indexPath);
                content = content.Replace(InjectAuthController.BlockToInject, "", StringComparison.Ordinal);
                File.WriteAllText(_indexPath, content);
                _logger.LogInformation("Successfully removed the JavaScript Injector script from index.html during uninstall.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while trying to remove script from index.html during uninstall.");
            }

            base.OnUninstalling();
        }


        /// <inheritdoc />
        public override string Name => Constants.PluginName;

        /// <inheritdoc />
        public override Guid Id => Guid.Parse(Constants.PluginGuid);

        public static Plugin Instance { get; private set; }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return
            [
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
                }
            ];
        }
    }
}

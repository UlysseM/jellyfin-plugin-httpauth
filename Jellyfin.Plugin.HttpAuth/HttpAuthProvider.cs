using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Jellyfin.Data;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Database.Implementations.Enums;
using Jellyfin.Plugin.HttpAuth.Configuration;
using MediaBrowser.Common;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HttpAuth
{
    // The purpose of this class is to login the user specified by Config.UserHeader (default is `X-Forwarded-User`).
    // 
    // Users that doesn't exist 
    public class HttpAuthProvider : IAuthenticationProvider
    {
        private readonly ILogger<HttpAuthProvider> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApplicationHost _applicationHost;

        public HttpAuthProvider(IApplicationHost applicationHost, ILogger<HttpAuthProvider> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _applicationHost = applicationHost;
        }
        public string Name => "HttpAuth";

        private static PluginConfiguration Config { get { return Plugin.Instance.Configuration; } }

        public bool IsEnabled { get { return Config.EnablePlugin; } }



        static public string GetUsernameFromRequest(HttpRequest httpReq)
        {
            if (!httpReq.Headers.TryGetValue(Config.UserHeader, out var users) || users.Count != 1)
            {
                return null;
            }
            return users[0];
        }

        public async Task<ProviderAuthenticationResult> Authenticate(string username, string password)
        {
            // We only do this if the username is set to HttpAuth.
            if (username != "HttpAuth")
            {
                throw new AuthenticationException("username invalid");
            }
            if (!Config.EnablePlugin)
            {
                _logger.LogError("Plugin not enabled. You need to explicitly enable the plugin.");
                throw new AuthenticationException("Plugin not enabled");
            }
            string user = GetUsernameFromRequest(_httpContextAccessor.HttpContext.Request);
            if (user == null) {
                _logger.LogInformation("Header {UserHeader} was not provided.", Config.UserHeader);
                throw new AuthenticationException($"Header {Config.UserHeader} was not provided.");
            }
            return await DoAuthentification(user);
        }

        private async Task<ProviderAuthenticationResult> DoAuthentification(string actualUsername)
        {
            var userManager = _applicationHost.Resolve<IUserManager>();
            // Create a user if it doesn't exist.
            if (userManager.GetUserByName(actualUsername) == null)
            {
                _logger.LogInformation("Creating user {ActualUsername} as it doesn't exist.", actualUsername);
                User user = await userManager.CreateUserAsync(actualUsername).ConfigureAwait(false);
                user.SetPermission(PermissionKind.IsAdministrator, Config.NewUsersAreAdmin);
                user.SetPermission(PermissionKind.EnableAllFolders, true);
                user.AuthenticationProviderId = GetType().FullName;
                await userManager.UpdateUserAsync(user).ConfigureAwait(false);
            }
            // Then, do the auth
            return new ProviderAuthenticationResult
            {
                Username = actualUsername
            };
        }

        public Task ChangePassword(User user, string newPassword)
        {
            throw new NotImplementedException();
        }

        public bool HasPassword(User user)
        {
            return true;
        }
    }
}

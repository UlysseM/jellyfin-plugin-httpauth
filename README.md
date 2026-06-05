<h1 align="center">Jellyfin HttpAuth Plugin</h1>

## About

This plugins allows users to skip the login page (and not set password in jellyfin), provided your app is configured to use a reverse proxy that provides a username through an HTTP header.

**Warning: If you allow connections to your jellyfin without a reverse proxy, or if your reverse proxy doesn't override that header, this plugin is NOT for you, as it would allow anyone to log in as any other user, admin included.**

## Installation

1. Modify your reverse proxy to provide a username through some HTTP Header of your choice (eg: `X-Forwarded-User`).

2. (Optional) If you're using docker, and you want to skip the login screen, you will need to allow the app to modify your index.html at startup. This can be done by adding the following `post_start` in your docker compose file.
```
services:
  jellyfin:
    # ... other config
    # the next 4 lines needs to be added to your config.
    post_start:
      - command: chmod -c 666 /jellyfin/jellyfin-web/index.html
        user: root
        privileged: true
    volumes:
      # ... other config
```

3. Add the following repository to your jellyfin repositories: `https://raw.githubusercontent.com/UlysseM/jellyfin-plugin-httpauth/`, install the plugin from the catalog.

4. After restarting jellyfin, head to the configuration page for the plugin, and check the box to enable the plugin. You may also decide whether you want non-existing users to be created as admin or not, and which HTTP Header you want the app to use (default being `X-Forwarded-User`).

5. You & all your users may need to refresh your cache (Ctrl + Shift + R) to have the updated index that injects the auto-login feature.

6. (Optional) In your "branding", you can mention something like "If you see this page, try Ctrl + Shift + R to login", or you can tell them to manually log in as user: "HttpHeader", password: *whatever they want to fill*. This will work as a backup in case the index.html didn't get modify.

7. That's it! If you log out of your account, and gets redirected to the home page, you should be logged back in almost instantly.

## How this plugin works

- Whenever a manual login attempt is made with the user `HttpAuth`, regardless of the password, an IAuthenticationProvider implementation will log the user as whatever the http header `X-Forwarded-User` (configurable) is set to.  If that user doesn't exist, it will be automatically created (with or without admin permission depending on the configuration).
- The html file will be modified when the plugin starts, to inject a javacsript file.
- That javacript file will be served by the plugin itself. It contains enough logic to detect when the signing page is live, and auto-fills the field with the right user `HttpAuth`, some random password, before logging in.

The design philosophy behind this plugin is to keep everything simple. The fact that I'm not exposing configuration to allow specific users to have specific permissions is not a missing feature; it's by design. I intend to make new release of this plugin only when necessary (eg: Jellyfin makes a breaking change to their codebase, or updates the web UI so the hook no longer works).

The very small codebase also allows for easy audits, which should reassure folks interested in giving this plugin a go. 

## Why this plugin exist

I originally started using the 9p4/jellyfin-plugin-sso. It worked great (thanks!) but it had some flaws for my usecase:
- It required setting up providers.
- There is one more button to login.
- The codebase has dependency on various libraries, making it more likely to get hit by vulnerabilities. This is aggravated by the fact that the plugin will no longer be receiving updates.

Which gets me to the point, since reverse proxy can pass in username through headers, the extra complexity can be skipped.

I also noticed there was some demand for such plugin; There was [an attempt](https://github.com/pikami/jellyfin-header-auth) at solving the same problem, but never worked and was abandoned, despite comment asking for status update.

Anyhow, I made this plugin to answer my specific need, not to gain experience in c# / .net. Based on my lack of experience in this framework, my code (file structure & comment) may appear sloppy to some, but it's good enough for me.


## I don't know much about HTTP Headers, how can I know more?

The TLDR is you can use reverse proxy like NGINX to authentificate the user through some 3rd party mechanism, and have nginx extract the username from it. You should then be able to write the username through a custom HTTP Header.

For instance, my custom setup hides jellyfin from the public internet, and I can access my jellyfin instance through:
- `mtls`, configured at the nginx level, which requires some setup and downloading certificate onto my devices. The username can then be obtained through `$ssl_client_s_dn_cn`.
- `proxy_pass`, using keycloak/oauth2 proxy, I'm able to capture the username through `$upstream_http_x_auth_request_preferred_username`.
- `wireguard`, my configuration assigns each user to a specific IP address on my server, and I'm using nginx `geo` to map specific IPs to username.

Lastly, you also want to make sure to block any other traffic that didn't originate from your reverse proxy, or anyone able to reach your instance without it could provide their own HTTP Header.

## Special thanks

I picked some ideas from the following jellyfin plugins: jellyfin/jellyfin-plugin-anilist, 9p4/jellyfin-plugin-sso and n00bcodr/Jellyfin-JavaScript-Injector.
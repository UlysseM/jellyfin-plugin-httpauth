<h1 align="center">Jellyfin HttpAuth Plugin</h1>

## About

This plugins allows users to automatically login through a http header, which MUST be provided by a proxy.

**Warning: This plugin will work great for those who knows exactly how to set up your environment, but if that's not you, you should certainly not use this plugin, as it would allow anyone to log in as admin on your jellyfin server.**

## Getting started

To install, add the package repo https://raw.githubusercontent.com/UlysseM/jellyfin-plugin-httpauth/gh-pages/repository.json to your repositories, then install the plugin from the plugin catalog!

You can then go to the configuration page to:
- Turn the plugin on (yes, I added another check to make people think again before they use this plugin).
- Update the header you with to use, for passing in the username (default being `X-Forwarded-User`).
- Decide whether or not users that don't already exist gets to be admin or not upon creation.

## How it works

This plugin is designed to be as small as possible, to be easily auditable. As such, while you can submit feature request, it's unlikely to be implemented.

- Using a IAuthenticationProvider, we will allow the login of the user specified by the http header `X-Forwarded-User` (configurable), if a manual login attempt is made with the user: `HttpAuth`, regardless of the password.
- If that user doesn't exist, it will be created on the fly (with or without admin depending on the configuration).
- The html file will be modified when the plugin start, to inject a javacsript file.
- That javacript file will be served by the plugin itself. It contains enough logic to detect when the signing page is live, and auto-fills the field with the right user / password, before logging in.


## Why this plugin exist

I originally started using the 9p4/jellyfin-plugin-sso. It works great (thanks!) but it had some flaws for my usecase:
- It requires setting up providers
- There is one more button to login
- The codebase has dependency on various libraries, which recently got vulnerabilities. With that plugin being archived, receiving security fixes will become more challenging... Even though it doesn't really matter for my usecase, as jellyfin is hidden behind a http proxy.

Which gets me to the point, the proxy can pass in a username as a header, and skip all of this complexity. There was another attempt on github made 4 years ago, but it never got anywhere, so I thought I'd make my own.

I haven't written c# in over 10 years, and it's my first time using .net; so things are probably not super pretty / standard / organized, but it's good enough for me.


## Closing thoughts

Again, this plugin may work great for you if you know what you're doing, otherwise, I'd advise against it. The only safe way to use this plugin is to place it behind a http proxy, and ensure that you control how the header is set.

My custom setup uses nginx as a proxy to hide jellyfin from to the public internet, with 3 authentification mechanism. Each is able to provide me with a username, that I can use to pass in as the http header to log in:
- `proxy_pass`, through keycloak/oauth2 proxy, I'm able to capture the username through `$upstream_http_x_auth_request_preferred_username`.
- `mtls`, through `$ssl_client_s_dn_cn`.
- `wireguard`, each client has its dedicated ip, and I'm using `geo` to map specific IPs to username.

I also made sure to block any traffic that wasn't going through nginx.

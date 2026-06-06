"use strict";

let listener;
let loginAttempted = false;

const attemptLogin = () => {
    // The manualLoginForm cannot be have the hide class
    if (document.querySelectorAll(".manualLoginForm.hide").length) {
        return false;
    }

    const userField = document.getElementById("txtManualName");
    const passField = document.getElementById("txtManualPassword");
    const submitBtn = document.querySelectorAll(".manualLoginForm button.button-submit");
    if (!userField || !passField || submitBtn.length != 1) {
        return false;
    }
    listener.disconnect();
    if (loginAttempted) {
        return false;
    }
    console.log("Doing the auto-login");

    loginAttempted = true;
    userField.value = "HttpAuth";
    // prevent password manager from trying to save this...
    passField.type = "text";
    // We don't care, just pick a random string.
    passField.value = Math.random().toString(36).substring(2, 13);
    submitBtn[0].click();
    return true;
}

listener = new MutationObserver(attemptLogin);

const changeUrl = (url) => {
    if (url.includes("#/login")) {
        if (!attemptLogin()) {
            listener.disconnect();
            listener.observe(document.getElementById("reactRoot"), { attributes: true, childList: true, subtree: true });
        }
    }
}


const setupCallback = async () => {
    try {
        const resp = await fetch('/injectauth/auth_data');
        if (!resp.ok) {
            throw new Error(`HTTP error! Status: ${resp.status}`);
        }
        const data = await resp.json();
        if (data.Username.length > 1) {
            window.httpAuthUserLogin = data.Username;
            if (data.Enabled) {
                // We got a username and the plugin is enabled, so we can add the hook that logs us in automatically.
                navigation.addEventListener("navigate", (event) => {
                    changeUrl(event.destination.url);
                });
                changeUrl(window.location.href);
            } else {
                throw new Error("The http auth plugin is disabled. An admin could turn it back in the configuration. The HTTP header works, providing the username:", data.Username);
            }
        } else {
            window.httpAuthUserLogin = false;
            if (data.Enabled) {
                throw new Error("The http auth plugin is enabled, but the HTTP header was not set. This is not secure. This message keeps appearing while the safety breaker is off.");
            } else {
                throw new Error("The http auth plugin is disabled, and no HTTP headers were provided. An admin should investigate the reverse proxy not sending the correct HTTP header.");
            }
        }
    } catch (error) {
        console.error("Error with HTTP Auth plugin:", error)
    }
}
setupCallback();

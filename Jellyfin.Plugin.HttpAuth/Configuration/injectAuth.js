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
    if (url.includes("web/#/login") || url.includes("web/?#/login")) {
        if (!attemptLogin()) {
            listener.disconnect();
            listener.observe(document.getElementById("reactRoot"), { attributes: true, childList: true, subtree: true });
        }
    }
}


document.addEventListener('DOMContentLoaded', (event) => {
    navigation.addEventListener("navigate", (event) => {
        changeUrl(event.destination.url);
    });
    changeUrl(window.location.href);
});

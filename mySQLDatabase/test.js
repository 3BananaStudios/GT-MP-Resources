/// <reference path="types-gt-mp/Declarations.d.ts" />
"use strict";
var myBrowser = null; //Set the browser to null as its not started yet
API.onResourceStart.connect(function () {
    var res = API.getScreenResolution(); //Gets the clients resoultion sotres it in a variable
    myBrowser = API.createCefBrowser(res.Width, res.Height); //Initializes CEF browser, for the full screen
    API.waitUntilCefBrowserInit(myBrowser); //Stops the script unitl the browser loads
    API.setCefBrowserPosition(myBrowser, 0, 0); //Locks the browser to the top-left
    API.loadPageCefBrowser(myBrowser, "myFile.html"); //Loads the HTML choice
    API.showCursor(true); // Shows the mosue cursor
    API.setCanOpenChat(false); // Stops the user from opening teh chat
});
function login(Username, Password) {
    API.sendChatMessage("Your username is " + Username + " and your password is " + Password);
    API.showCursor(false); //stop showing the cursor
    API.destroyCefBrowser(myBrowser); //destroy the CEF browser
    API.setCanOpenChat(true); //allow the player to use the chat again.
}

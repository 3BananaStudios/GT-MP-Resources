class CefHelper {
    constructor(resourcePath) {
        this.path = resourcePath
        this.open = false
    }

    show() {
        if (this.open === false) {
            this.open = true

            var resolution = API.getScreenResolution()

            this.browser = API.createCefBrowser(resolution.Width, resolution.Height, true)
            API.waitUntilCefBrowserInit(this.browser)
            API.setCefBrowserPosition(this.browser, 0, 0)
            API.loadPageCefBrowser(this.browser, this.path)
            API.showCursor(true)
            API.setCanOpenChat(false)
        }
    }

    destroy() {
        this.open = false
        API.destroyCefBrowser(this.browser)
        API.showCursor(false)
        API.setCanOpenChat(true)
    }

    eval(string) {
        this.browser.eval(string)
    }
}

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {

        case 'successfulLogin':
            API.sendNotification('~g~Succesfully logged in')
            break
        case 'failedLogin':
            API.sendNotification('~r~Login failed! One or more fields are incorrect. Try again...')
            ShowLogin()
            break
        case 'successfulRegister':
            API.sendNotification('~g~Successfully Registered')
            break
        case 'failedRegisterUserExists':
            API.sendNotification("~r~Registering failed an account with that name already exists... Try a different one!")
            ShowRegister()
            break
    }
})

var cef = null;
API.onKeyDown.connect(function (sender, e) {
    if (cef === null && e.KeyCode === Keys.Up) {
        cef = new CefHelper("client/resources/welcomescreen.html")
        cef.show()
    }

    if (e.KeyCode == Keys.Down) {
        cef.destroy()
        cef = null
    }
})

API.onResourceStart.connect(function ()
{
    cef = new CefHelper("client/resources/welcomescreen.html")
    cef.show()
    API.waitUntilCefBrowserLoaded(cef.browser)
})

function ShowLogin()
{
    if (cef != null)
    {
        cef.destroy()
        cef = null
    }

    cef = new CefHelper("client/resources/Login.html")
    cef.show()
}

function ShowRegister()
{
    if (cef != null) {
        cef.destroy()
        cef = null
    }

    cef = new CefHelper("client/resources/Register.html")
    cef.show()
    
}

function Login(username, password)
{
    cef.destroy()
    cef = null
    API.triggerServerEvent("Login", username, password)
}

function Register(username, password)
{
    cef.destroy()
    cef = null
    API.triggerServerEvent("Register", username, password)
}


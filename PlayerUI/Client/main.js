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

moneyText = null

API.onResourceStart.connect(function () {
    
   
})

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName)
    {
        case 'showMoney':
            //moneyText = API.addTextElement("$1000", 100.0, 200.0, 1.0, 0, 255, 0, 255, 7, -1)
            API.drawText("test", 100.0, 200.0, 1.0, 255, 255, 255, 255, 1, 0, true, true, 45)
            break
        
    }
})
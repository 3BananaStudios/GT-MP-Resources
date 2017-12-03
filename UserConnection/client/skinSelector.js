/// <reference path="../types-gt-mp/Definitions/index.d.ts" />
var skins = [-1734476390, -680474188, 1644266841, -412008429, 68070371, -1868718465];
var currentSkin = 0;
var selecting = false;
var originalCamera = null;
API.onUpdate.connect(function () {
    if (selecting) {
        API.disableAllControlsThisFrame();
    }
});
function SkinSelector() {
    API.setPlayerSkin(skins[0]);
    API.setEntityPosition(API.getLocalPlayer(), new Vector3(-171.57, -665.46, 40.48), true);
    FocusCameraInfrontOfPlayer();
}
function FocusCameraInfrontOfPlayer() {
    originalCamera = API.getActiveCamera();
    var player = API.getLocalPlayer();
    var playerPos = API.getEntityPosition(player);
    var playerRot = API.getEntityRotation(player);
    var newCamera = API.createCamera(playerPos.Add(new Vector3(0, 2.5, 0)), playerRot.Add(new Vector3(0, 0, 180)));
    API.setActiveCamera(newCamera);
}
function chooseNextSkin() {
    currentSkin++;
    currentSkin = clamp(currentSkin, 0, skins.length - 1);
    var selectedSkin = skins[currentSkin];
    API.sendChatMessage(currentSkin.toString());
    API.setPlayerSkin(selectedSkin);
}
function choosePreviousSkin() {
    currentSkin--;
    currentSkin = clamp(currentSkin, 0, skins.length - 1);
    var selectedSkin = skins[currentSkin];
    API.sendChatMessage(currentSkin.toString());
    API.setPlayerSkin(selectedSkin);
}
function clamp(val, min, max) {
    return Math.max(min, Math.min(max, val));
}
//# sourceMappingURL=skinSelector.js.map
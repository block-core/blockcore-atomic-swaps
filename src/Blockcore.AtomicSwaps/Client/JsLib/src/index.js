import { WebProvider } from "@blockcore/provider";

window.signmessage = async function () {
    const webProvider = await WebProvider.Create();
    const msg = document.getElementById("blockcore-extension-input").value;
    const signing1 = await webProvider.request({ method: "signMessage", params: [{ key: "CdnpNqSeqaXBMVnY1e13Ksijgr6FyWPM9J", message: msg }] });
    document.getElementById("blockcore-extension-output").innerText = JSON.stringify(signing1);
}
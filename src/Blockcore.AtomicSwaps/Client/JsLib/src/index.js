import { WebProvider } from "@blockcore/provider";




window.signmessage = async function () {
    const webProvider = await WebProvider.Create();
    const msg = document.getElementById("blockcore-extension-input").value;
    const signing1 = await webProvider.request({ method: "signMessage", params: [{ key: "CdnpNqSeqaXBMVnY1e13Ksijgr6FyWPM9J", message: msg }] });
    document.getElementById("blockcore-extension-output").innerText = JSON.stringify(signing1);
}





window.connect = async function () {
    document.getElementById("blockcore-extension-output").innerText = "";

    const blockcore = globalThis.blockcore;

    if (!blockcore) {
        alert("The Blockcore provider is not available. Unable to continue.");
        return;
    }

    let pubKey;

    try {
        pubKey = await blockcore.request({ method: "publicKey" });
    } catch (err) {
        console.error(err);
        pubKey = "Error: " + err.message;
    }

    console.log("BLOCKCORE REQUEST EXECUTED:", pubKey);

    document.getElementById("blockcore-extension-output").innerText = pubKey;
}

window.connect2 = async function () {
    document.getElementById("blockcore-extension-output").innerText = "";

    const blockcore = globalThis.blockcore;

    if (!blockcore) {
        alert("The Blockcore provider is not available. Unable to continue.");
        return;
    }

    let signature;

    const msgParams = JSON.stringify({
        types: {
            EIP712Domain: [
                { name: "name", type: "string" },
                { name: "version", type: "string" },
                { name: "chainId", type: "uint256" },
                { name: "verifyingContract", type: "address" },
            ],
            // Not an EIP712Domain definition
            Bid: [{ name: "name", type: "string" }],
            // Refer to PrimaryType
            Mail: [
                { name: "from", type: "Person" },
                { name: "to", type: "Person[]" },
                { name: "contents", type: "string" },
            ],
            // Not an EIP712Domain definition
            Person: [
                { name: "name", type: "string" },
                { name: "wallets", type: "address[]" },
            ],
        },
    });

    try {
        signature = await blockcore.request({ method: "sign", params: [msgParams] });
    } catch (err) {
        console.error(err);
        signature = "Error: " + err.message;
    }

    document.getElementById("blockcore-extension-output").innerText = signature;
}

window.connect3 = async function () {
    document.getElementById("blockcore-extension-output").innerText = "";
    const msg = document.getElementById("blockcore-extension-input").value;

    const blockcore = globalThis.blockcore;

    if (!blockcore) {
        alert("The Blockcore provider is not available. Unable to continue.");
        return;
    }

    let signature;

    try {
        signature = await blockcore.request({ method: "signMessage", params: [{ scheme: "schnorr", message: msg }] });
    } catch (err) {
        console.error(err);
        signature = "Error: " + err.message;
    }

    document.getElementById("blockcore-extension-output").innerText = JSON.stringify(signature);
}

window.connect4 = async function () {
    document.getElementById("blockcore-extension-output").innerText = "";

    const blockcore = globalThis.blockcore;

    if (!blockcore) {
        alert("The Blockcore provider is not available. Unable to continue.");
        return;
    }

    let signature;
    const msgParams = "{ name: 'John Doe' }";
    const signatureScheme = "schnorr";

    try {
        signature = await blockcore.request({ method: "signVerifiableCredential", params: [signatureScheme, msgParams] });
    } catch (err) {
        console.error(err);
        signature = "Error: " + err.message;
    }

    document.getElementById("blockcore-extension-output").innerText = signature;
}

window.connect5 = async function () {
    document.getElementById("blockcore-extension-output").innerText = "";
    const msg = document.getElementById("blockcore-extension-input").value;

    const blockcore = globalThis.blockcore;
    let signature;

    try {
        signature = await blockcore.request({ method: "signMessage", params: [{ key: "CdnpNqSeqaXBMVnY1e13Ksijgr6FyWPM9J", scheme: "schnorr", message: msg }] });
    } catch (err) {
        console.error(err);
        signature = "Error: " + err.message;
    }

    document.getElementById("blockcore-extension-output").innerText = JSON.stringify(signature);
}


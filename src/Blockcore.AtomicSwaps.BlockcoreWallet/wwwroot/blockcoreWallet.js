export async function hasBlockcoreWallet() {
    return (globalThis.blockcore != undefined);
}

export async function signMessage(msg) {
    const blockcore = globalThis.blockcore;
    let signature;
    try {
        signature = await blockcore.request({ method: "signMessage", params: [{ scheme: "schnorr", message: msg }] });
        return JSON.stringify(signature);
    }
    catch (error) {
        console.error("Error: " + error.message);
        // User denied account access...
        throw "UserDenied";
    }
}



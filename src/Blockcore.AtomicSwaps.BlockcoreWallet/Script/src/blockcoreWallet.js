import { BlockcoreDns } from '@blockcore/dns';
import { WebProvider } from '@blockcore/provider';

export async function hasBlockcoreWallet() {
    return (globalThis.blockcore != undefined);
}

export async function signMessage(msg) {
    const provider = await WebProvider.Create();
    let signature;
    try {
        signature = await provider.request({ method: "signMessage", params: [{ scheme: "schnorr", message: msg }] });
        return JSON.stringify(signature);
    }
    catch (error) {
        console.error("Error: " + error.message);
        // User denied account access...
        throw "UserDenied";
    }
}


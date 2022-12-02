import { BlockcoreDns } from './_snowpack/pkg/@blockcore/dns.js';
import { WebProvider } from './_snowpack/pkg/@blockcore/provider.js';

const network = 'BTC';

export async function hasBlockcoreWallet() {
    return (globalThis.blockcore != undefined);
}

export async function signMessageAnyAccount(value) {
    const provider = await WebProvider.Create();
    provider.setNetwork(network);

    const result = await provider.request({
        method: 'signMessage',
        params: [{ message: value, network: provider.indexer.network }],
    });
    console.log('Result:', result);
    return JSON.stringify(result);
    //var key = result.key;
    //var signature = result.signature;
    //var network = result.network;
    //var verify = bitcoinMessage.verify(value, result.key, result.signature);
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






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

export async function signMessageAnyAccountJson(value) {
    const message = JSON.parse(value);

    const provider = await WebProvider.Create();

    const result = await provider.request({
        method: 'signMessage',
        params: [{ message: message, network: provider?.indexer.network }],
    });

    console.log('Result:', result);
    return JSON.stringify(result);

    //this.signedJsonKey = result.key;
    //this.signedJsonSignature = result.signature;
    //this.signedJsonNetwork = result.network;
    //const preparedMessage = JSON.stringify(message);
    //this.signedJsonValidSignature = bitcoinMessage.verify(preparedMessage, result.key, result.signature);
}

export async function paymentRequest(network, amount) {
    try {
        const provider = await WebProvider.Create();

        var result = await provider.request({
            method: 'payment',
            params: [
                {
                    network: network.toLowerCase(),
                    amount: amount,
                    address: 'Ccoquhaae7u6ASqQ5BiYueASz8EavUXrKn',
                    label: 'Your Local Info',
                    message: 'Invoice Number 5',
                    data: 'MzExMzUzNDIzNDY',
                    id: '4324',
                },
            ],
        });

        console.log('Result:', result);
        return JSON.stringify(result);
    } catch (err) {
        console.error(err);
    }
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






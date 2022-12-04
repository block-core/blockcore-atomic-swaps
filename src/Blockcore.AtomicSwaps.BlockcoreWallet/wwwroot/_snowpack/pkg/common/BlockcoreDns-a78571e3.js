class WebRequest {
    static async fetchText(url) {
        const response = await WebRequest.fetchUrl(url);
        return response.text();
    }
    static async fetchJson(url) {
        const response = await WebRequest.fetchUrl(url);
        return response.json();
    }
    static async fetchUrl(url) {
        return await fetch(url, {
            method: 'GET',
            mode: 'cors',
            cache: 'no-cache',
            credentials: 'omit',
            redirect: 'follow',
            referrerPolicy: 'no-referrer',
        });
    }
}

class BlockcoreDnsClient {
    constructor(nameserver) {
        Object.defineProperty(this, "activeServer", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        this.setActiveServer(nameserver);
    }
    setActiveServer(url) {
        this.activeServer = url;
    }
    async getServicesByType(service) {
        const url = `${this.activeServer}/api/dns/services/service/${service}`;
        return await WebRequest.fetchJson(url);
    }
    async getServicesByTypeAndNetwork(service, symbol) {
        const url = `${this.activeServer}/api/dns/services/symbol/${symbol}/service/${service}`;
        return await WebRequest.fetchJson(url);
    }
    async getServicesByNetwork(symbol) {
        const url = `${this.activeServer}/api/dns/services/symbol/${symbol}`;
        return await WebRequest.fetchJson(url);
    }
    async getExternalIP() {
        const url = `${this.activeServer}/api/dns/ipaddress`;
        return await WebRequest.fetchText(url);
    }
}

/** The BlockcoreDns class will give you access to all known nameservers and all services registered with those nameservers. */
class BlockcoreDns {
    constructor() {
        Object.defineProperty(this, "nameservers", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        Object.defineProperty(this, "services", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        Object.defineProperty(this, "api", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        this.api = new BlockcoreDnsClient('');
    }
    async getDnsServers() {
        const url = `https://chains.blockcore.net/services/DNS.json`;
        const servers = await WebRequest.fetchJson(url);
        return servers;
    }
    getNameServers() {
        return this.nameservers;
    }
    getOnlineServicesByNetwork(network) {
        return this.services.filter((s) => s.symbol === network && s.online === true);
    }
    getServices() {
        return this.services;
    }
    /** Attempts to load the latest status of all services from all known nameservers. This method can be called
     * at intervals to ensure latest status is available.
     *
     * By supplying the serviceType, a different than default list of services can be provided in the .getServices() method.
     *
     * Supply null as parameter for serviceType to avoid preloading services.
     */
    async load(nameservers, serviceType = 'Indexer') {
        this.nameservers = nameservers || (await this.getDnsServers());
        const servicesMap = new Map();
        for (let i = 0; i < this.nameservers.length; i++) {
            const nameserver = this.nameservers[i];
            if (!nameserver) {
                continue;
            }
            this.api.setActiveServer(nameserver.url);
            if (serviceType) {
                const services = await this.api.getServicesByType(serviceType);
                services.forEach((item) => servicesMap.set(item.domain, { ...servicesMap.get(item.domain), ...item }));
            }
        }
        this.services = Array.from(servicesMap.values());
        // Set randomly active server after load is complete.
        const randomIndex = this.getRandomInt(this.nameservers.length);
        this.api.setActiveServer(this.nameservers[randomIndex].url);
    }
    getRandomInt(max) {
        return Math.floor(Math.random() * max);
    }
}

export { BlockcoreDns as B, WebRequest as W };

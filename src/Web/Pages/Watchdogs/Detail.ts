import { Controller } from "@hotwired/stimulus";

export default class DetailController extends Controller {
    static targets = [
        "webPagesToMonitor"
    ];

    static values = {
        webPageToMonitorUrl: String
    };
    
    declare webPagesToMonitorTarget: HTMLDivElement;

    declare webPageToMonitorUrlValue: string;

    async addWebPageToMonitor() {
        const response = await fetch(this.webPageToMonitorUrlValue);
        if (response.ok) {
            const html = await response.text();
            this.webPagesToMonitorTarget.insertAdjacentHTML("beforeend", html);
        }
    }
}
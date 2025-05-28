import { Controller } from "@hotwired/stimulus";
import BaseStimulusModelController from "../BaseStimulusModelController";
import { DetailStimulusModel } from "../Shared/Generated/DetailStimulusModel";

export default class DetailController extends BaseStimulusModelController<DetailStimulusModel> {
    static targets = [
        "webPagesToMonitor"
    ];
   
    declare webPagesToMonitorTarget: HTMLDivElement;

    async addWebPageToMonitor() {
        const response = await fetch(this.modelValue.webPageToMonitorUrl);
        if (response.ok) {
            const html = await response.text();
            this.webPagesToMonitorTarget.insertAdjacentHTML("beforeend", html);
        }
    }
}
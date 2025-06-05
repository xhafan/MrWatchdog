import { Controller } from "@hotwired/stimulus";
import BaseStimulusModelController from "../../Shared/BaseStimulusModelController";
import { DetailStimulusModel } from "../../Shared/Generated/DetailStimulusModel";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import Enumerable from "linq";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";

export default class DetailController extends BaseStimulusModelController<DetailStimulusModel> {
    static targets = [
        "webPagesToMonitor",
        "addWebPageToMonitorForm"
    ];
   
    declare webPagesToMonitorTarget: HTMLDivElement;
    declare addWebPageToMonitorFormTarget: HTMLFormElement;

    connect() {
        const addWebPageToMonitorForm = this.addWebPageToMonitorFormTarget;

        formSubmitWithWaitForJobCompletion(
            addWebPageToMonitorForm, 
            this.modelValue.getJobUrl, 
            async jobDto => {
                const watchdogWebPageEntity = Enumerable.from(jobDto.affectedEntities)
                    .singleOrDefault(x => x.entityName === DomainConstants.watchdogWebPage && x.isCreated);
                if (!watchdogWebPageEntity) {
                    throw new Error("Error getting created WatchdogWebPage.");
                }

                const webPageToMonitorTurboFrameUrl = this.modelValue.webPageToMonitorTurboFrameUrl.replace("$watchdogWebPageId", String(watchdogWebPageEntity.entityId));

                const response = await fetch(webPageToMonitorTurboFrameUrl);
                if (response.ok) {
                    const html = await response.text();
                    this.webPagesToMonitorTarget.insertAdjacentHTML("beforeend", html);
                }
            }
        );
    }
}
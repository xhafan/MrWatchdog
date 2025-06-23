import { Controller } from "@hotwired/stimulus";
import BaseStimulusModelController from "../../Shared/BaseStimulusModelController";
import { DetailStimulusModel } from "../../Shared/Generated/DetailStimulusModel";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import Enumerable from "linq";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import { watchdogWebPageRemovedEvent } from "./WebPage/WebPageController";

export default class DetailController extends BaseStimulusModelController<DetailStimulusModel> {
    static targets = [
        "webPages",
        "addWebPageForm",
        "webPageTurboFrame"
    ];
   
    declare webPagesTarget: HTMLDivElement;
    declare addWebPageFormTarget: HTMLFormElement;
    declare webPageTurboFrameTargets: HTMLFormElement[];

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.addWebPageFormTarget, 
            async jobDto => {
                const watchdogWebPageEntity = Enumerable.from(jobDto.affectedEntities)
                    .singleOrDefault(x => x.entityName === DomainConstants.watchdogWebPageEntityName && x.isCreated);
                if (!watchdogWebPageEntity) {
                    throw new Error("Error getting created WatchdogWebPage.");
                }

                const webPageTurboFrameUrl = this.modelValue.webPageTurboFrameUrl.replace("$watchdogWebPageId", String(watchdogWebPageEntity.entityId));

                const response = await fetch(webPageTurboFrameUrl);
                if (response.ok) {
                    const html = await response.text();
                    this.webPagesTarget.insertAdjacentHTML("beforeend", html);
                }
            }
        );

        this.element.addEventListener(watchdogWebPageRemovedEvent, this.onWatchdogWebPageRemoved.bind(this));
    }

    private onWatchdogWebPageRemoved(event: CustomEventInit) {
        let watchdogWebPageElement = event.detail as HTMLElement;
     
        const parentTurboFrame = this.webPageTurboFrameTargets.find(
            webPageTurboFrame => webPageTurboFrame.contains(watchdogWebPageElement)
        );

        if (!parentTurboFrame) throw new Error("Parent turbo-frame not found for the removed watchdog web page.");
        parentTurboFrame.remove();
    }
}
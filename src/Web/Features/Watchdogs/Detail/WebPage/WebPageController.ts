import { Controller } from "@hotwired/stimulus";
import { FrameElement } from "@hotwired/turbo";
import { formSubmitWithWaitForJobCompletion, getRelatedDomainEventJobGuid, waitForJobCompletion } from "../../../Jobs/jobCompletion";
import { formSubmitJobCompletedEventName } from "../../../Shared/TagHelpers/ViewOrEditForm/ViewOrEditFormController";
import { DomainConstants } from "../../../Shared/Generated/DomainConstants";
import { JobDto } from "../../../Shared/Generated/JobDto";
import { watchdogWebPageNameModifiedEventName } from "./WebPageOverviewController";

export const watchdogWebPageRemovedEvent = "watchdogWebPageRemoved";

export default class WebPageController extends Controller {
    static targets = [
        "removeWebPageForm",
        "webPageOverview",
        "webPageSelectedHtml",
        "webPageName"
    ];
   
    declare removeWebPageFormTarget: HTMLFormElement;
    declare webPageOverviewTarget: FrameElement;
    declare webPageSelectedHtmlTarget: FrameElement;
    declare webPageNameTarget: HTMLSpanElement;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.removeWebPageFormTarget, 
            async jobDto => {
                this.element.dispatchEvent(new CustomEvent(watchdogWebPageRemovedEvent, { bubbles: true, detail: this.element }));
            },
            "Really remove the web page to monitor?"
        );

        this.webPageOverviewTarget.addEventListener(formSubmitJobCompletedEventName, this.onUpdateWatchdogWebPageJobCompleted.bind(this), {});
        this.webPageOverviewTarget.addEventListener(watchdogWebPageNameModifiedEventName, this.onWatchdogWebPageNameModified.bind(this), {});
    }

    private async onUpdateWatchdogWebPageJobCompleted(event: CustomEventInit<JobDto>) {
        let updateWatchdogWebPageJobDto = event.detail;
        if (!updateWatchdogWebPageJobDto) throw new Error("JobDto is missing.");
        let updateWatchdogWebPageJobGuid = updateWatchdogWebPageJobDto.guid;

        var watchdogWebPageUpdatedDomainEventJobGuid = await getRelatedDomainEventJobGuid(updateWatchdogWebPageJobGuid, DomainConstants.watchdogWebPageUpdatedDomainEvent);
        
        if (!watchdogWebPageUpdatedDomainEventJobGuid) return;

        var watchdogWebPageUpdatedDomainEventJobDto = await waitForJobCompletion(watchdogWebPageUpdatedDomainEventJobGuid);

        this.webPageSelectedHtmlTarget.reload();
    }

    private async onWatchdogWebPageNameModified(event: CustomEventInit<string>) {
        let watchdogWebPageName = event.detail;
        if (!watchdogWebPageName) throw new Error("Watchdog web page name is missing.");
        this.webPageNameTarget.textContent = watchdogWebPageName;
    }
}
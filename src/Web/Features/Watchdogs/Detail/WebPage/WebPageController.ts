import { Controller } from "@hotwired/stimulus";
import { FrameElement } from "@hotwired/turbo";
import { formSubmitWithWaitForJobCompletion, getRelatedDomainEventJobGuid, waitForJobCompletion } from "../../../Jobs/jobCompletion";
import { formSubmitJobCompletedEventName } from "../../../Shared/TagHelpers/ViewOrEditForm/ViewOrEditFormController";
import { DomainConstants } from "../../../Shared/Generated/DomainConstants";
import { JobDto } from "../../../Shared/Generated/JobDto";
import { watchdogWebPageNameModifiedEventName } from "./WebPageOverviewController";
import { logError } from "../../../Shared/logging";
import { watchdogWebPageScrapedEvent } from "./WebPageScrapingResultsController";

export const watchdogWebPageRemovedEvent = "watchdogWebPageRemoved";

export default class WebPageController extends Controller {
    static targets = [
        "removeWebPageForm",
        "webPageOverview",
        "webPageScrapingResults",
        "webPageName",
        "webPageDisabledWarning"
    ];
   
    declare removeWebPageFormTarget: HTMLFormElement;
    declare webPageOverviewTarget: FrameElement;
    declare webPageScrapingResultsTarget: FrameElement;
    declare webPageNameTarget: HTMLSpanElement;
    declare webPageDisabledWarningTarget: FrameElement;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.removeWebPageFormTarget, 
            async jobDto => {
                this.element.dispatchEvent(new CustomEvent(watchdogWebPageRemovedEvent, { bubbles: true, detail: this.element }));
            },
            "Really remove the web page to watch?"
        );

        this.webPageOverviewTarget.addEventListener(formSubmitJobCompletedEventName, this.onUpdateWatchdogWebPageJobCompleted.bind(this), {});
        this.webPageOverviewTarget.addEventListener(watchdogWebPageNameModifiedEventName, this.onWatchdogWebPageNameModified.bind(this), {});
        this.element.addEventListener(watchdogWebPageScrapedEvent, this.onWatchdogWebPageScraped.bind(this), {});
    }

    private async onUpdateWatchdogWebPageJobCompleted(event: CustomEventInit<JobDto>) {
        let updateWatchdogWebPageJobDto = event.detail;
        if (!updateWatchdogWebPageJobDto) throw new Error("JobDto is missing.");
        let updateWatchdogWebPageJobGuid = updateWatchdogWebPageJobDto.guid;

        var watchdogWebPageScrapingDataUpdatedDomainEventJobGuid = 
            await getRelatedDomainEventJobGuid(updateWatchdogWebPageJobGuid, DomainConstants.watchdogWebPageScrapingDataUpdatedDomainEventName);
        
        if (!watchdogWebPageScrapingDataUpdatedDomainEventJobGuid) return;

        try {
            var watchdogWebPageUpdatedDomainEventJobDto = await waitForJobCompletion(watchdogWebPageScrapingDataUpdatedDomainEventJobGuid);

            this.webPageScrapingResultsTarget.reload();
            this.webPageDisabledWarningTarget.reload();
        }
        catch (error) {
            await logError(error, {}, true, true);
        }
    }

    private onWatchdogWebPageNameModified(event: CustomEventInit<string>) {
        let watchdogWebPageName = event.detail;
        this.webPageNameTarget.textContent = watchdogWebPageName ?? "";
    }

    private onWatchdogWebPageScraped(event: CustomEventInit<string>) {
        this.webPageDisabledWarningTarget.reload();
    }
}
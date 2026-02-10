import { FrameElement } from "@hotwired/turbo";
import { formSubmitWithWaitForJobCompletion, getRelatedDomainEventJobGuid, waitForJobCompletion } from "../../../Jobs/jobCompletion";
import { formSubmitJobCompletedEventName } from "../../../Shared/TagHelpers/ViewOrEditForm/ViewOrEditFormController";
import { DomainConstants } from "../../../Shared/Generated/DomainConstants";
import { JobDto } from "../../../Shared/Generated/JobDto";
import { scraperWebPageNameModifiedEventName } from "./WebPageOverviewController";
import { logError } from "../../../Shared/logging";
import { scraperWebPageScrapedEvent } from "./WebPageScrapedResultsController";
import BaseStimulusModelController from "../../../Shared/BaseStimulusModelController";
import { WebPageStimulusModel } from "../../../Shared/Generated/WebPageStimulusModel";

export const scraperWebPageRemovedEvent = "scraperWebPageRemoved";

export default class WebPageController extends BaseStimulusModelController<WebPageStimulusModel> {
    static targets = [
        "removeWebPageForm",
        "webPageOverview",
        "webPageScrapedResults",
        "webPageName",
        "webPageDisabledWarning"
    ];
   
    declare removeWebPageFormTarget: HTMLFormElement;
    declare webPageOverviewTarget: FrameElement;
    declare webPageScrapedResultsTarget: FrameElement;
    declare webPageNameTarget: HTMLSpanElement;
    declare webPageDisabledWarningTarget: FrameElement;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.removeWebPageFormTarget, 
            async jobDto => {
                this.element.dispatchEvent(new CustomEvent(scraperWebPageRemovedEvent, { bubbles: true, detail: this.element }));
            },
            this.modelValue.removeWebPageConfirmationMessageResource
        );

        this.webPageOverviewTarget.addEventListener(formSubmitJobCompletedEventName, this.onUpdateScraperWebPageJobCompleted.bind(this), {});
        this.webPageOverviewTarget.addEventListener(scraperWebPageNameModifiedEventName, this.onScraperWebPageNameModified.bind(this), {});
        this.element.addEventListener(scraperWebPageScrapedEvent, this.onScraperWebPageScraped.bind(this), {});
    }

    private async onUpdateScraperWebPageJobCompleted(event: CustomEventInit<JobDto>) {
        let updateScraperWebPageJobDto = event.detail;
        if (!updateScraperWebPageJobDto) throw new Error("JobDto is missing.");
        let updateScraperWebPageJobGuid = updateScraperWebPageJobDto.guid;

        var scraperWebPageScrapingDataUpdatedDomainEventJobGuid = 
            await getRelatedDomainEventJobGuid(updateScraperWebPageJobGuid, DomainConstants.scraperWebPageScrapingDataUpdatedDomainEventName);
        
        if (!scraperWebPageScrapingDataUpdatedDomainEventJobGuid) return;

        try {
            var scraperWebPageUpdatedDomainEventJobDto = await waitForJobCompletion(scraperWebPageScrapingDataUpdatedDomainEventJobGuid);

            this.webPageScrapedResultsTarget.reload();
            this.webPageDisabledWarningTarget.reload();
        }
        catch (error) {
            await logError(error, {}, true, true);
        }
    }

    private onScraperWebPageNameModified(event: CustomEventInit<string>) {
        let scraperWebPageName = event.detail;
        this.webPageNameTarget.textContent = scraperWebPageName ?? "";
    }

    private onScraperWebPageScraped(event: CustomEventInit<string>) {
        this.webPageDisabledWarningTarget.reload();
    }
}
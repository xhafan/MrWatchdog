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

    declare boundOnUpdateScraperWebPageJobCompleted: (event: CustomEventInit<JobDto>) => Promise<void>;
    declare boundOnScraperWebPageNameModified: (event: CustomEventInit<string>) => void;
    declare boundOnScraperWebPageScraped: (event: CustomEventInit<string>) => void;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.removeWebPageFormTarget, 
            async jobDto => {
                this.element.dispatchEvent(new CustomEvent(scraperWebPageRemovedEvent, { bubbles: true, detail: this.element }));
            },
            this.modelValue.removeWebPageConfirmationMessageResource
        );

        this.attachEventListeners();
    }

    disconnect() {
        this.removeEventListeners();
    }

    private attachEventListeners() {
        this.boundOnUpdateScraperWebPageJobCompleted = this.onUpdateScraperWebPageJobCompleted.bind(this);
        this.boundOnScraperWebPageNameModified = this.onScraperWebPageNameModified.bind(this);
        this.boundOnScraperWebPageScraped = this.onScraperWebPageScraped.bind(this);

        this.webPageOverviewTarget.addEventListener(formSubmitJobCompletedEventName, this.boundOnUpdateScraperWebPageJobCompleted);
        this.webPageOverviewTarget.addEventListener(scraperWebPageNameModifiedEventName, this.boundOnScraperWebPageNameModified);
        this.element.addEventListener(scraperWebPageScrapedEvent, this.boundOnScraperWebPageScraped);
    }

    private removeEventListeners() {
        this.webPageOverviewTarget.removeEventListener(formSubmitJobCompletedEventName, this.boundOnUpdateScraperWebPageJobCompleted);
        this.webPageOverviewTarget.removeEventListener(scraperWebPageNameModifiedEventName, this.boundOnScraperWebPageNameModified);
        this.element.removeEventListener(scraperWebPageScrapedEvent, this.boundOnScraperWebPageScraped);
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
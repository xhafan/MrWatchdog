import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../../Jobs/jobCompletion";
import BaseStimulusModelController from "../../../Shared/BaseStimulusModelController";
import { WebPageScrapedResultsStimulusModel } from "../../../Shared/Generated/WebPageScrapedResultsStimulusModel";

export const scraperWebPageScrapedEvent = "scraperWebPageScraped";

export default class WebPageScrapedResultsController extends BaseStimulusModelController<WebPageScrapedResultsStimulusModel> {
    static targets = [
        "scrapeWebPageForm",
        "scrapedOnInBrowserLocale"
    ];
   
    declare scrapeWebPageFormTarget: HTMLFormElement;
    
    declare scrapedOnInBrowserLocaleTarget: HTMLSpanElement;
    declare hasScrapedOnInBrowserLocaleTarget: boolean;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.scrapeWebPageFormTarget,
            async jobDto => {
                const turboFrame = this.element.closest("turbo-frame");
                turboFrame?.reload();

                this.element.dispatchEvent(new CustomEvent(scraperWebPageScrapedEvent, { bubbles: true }));
            }
        );
        if (this.hasScrapedOnInBrowserLocaleTarget) {
            this.scrapedOnInBrowserLocaleTarget.textContent = new Date(this.modelValue.scrapedOnInIso8601Format).toLocaleString();
        }
    }
}
import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../../Jobs/jobCompletion";
import BaseStimulusModelController from "../../../Shared/BaseStimulusModelController";
import { WebPageScrapingResultsStimulusModel } from "../../../Shared/Generated/WebPageScrapingResultsStimulusModel";

export const scraperWebPageScrapedEvent = "scraperWebPageScraped";

export default class WebPageScrapingResultsController extends BaseStimulusModelController<WebPageScrapingResultsStimulusModel> {
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
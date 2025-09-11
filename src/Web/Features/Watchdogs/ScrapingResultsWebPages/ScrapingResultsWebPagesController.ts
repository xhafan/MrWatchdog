import { Controller } from "@hotwired/stimulus";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../Shared/BodyController";
import { watchdogScrapingResultsWebPagesInitializedEventName } from "../ScrapingResults/ScrapingResultsController";

export const searchTermModifiedEventName = "searchTermModified";

export default class ScrapingResultsWebPagesController extends Controller {
    static targets = [
        "scrapingResult",
    ];
   
    declare scrapingResultTargets: HTMLElement[];

    connect() {
        this.registerSearchTermModifiedEventHandler();

        this.dispatch(watchdogScrapingResultsWebPagesInitializedEventName, { prefix: "" });
    }

    private registerSearchTermModifiedEventHandler() {
        this.dispatch(registerGlobalEventHandlerEventName, {
            prefix: "",
            detail: new EventHandlerRegistration<string>(searchTermModifiedEventName, this.handleSearchTermModifiedEvent.bind(this))
        });
    }

    private handleSearchTermModifiedEvent(event: CustomEventInit<string>) {
        const searchTerm = event.detail;

        this.scrapingResultTargets.forEach(scrapingResult => {
            if (!scrapingResult.textContent) {
                throw new Error(`Scraping result (${scrapingResult}) does not contain any text.`);
            }
            if (searchTerm && !this.includesIgnoringDiacritics(scrapingResult.textContent, searchTerm)) {
                scrapingResult.style.display = "none";
            }
            else {
                scrapingResult.style.display = "list-item";
            }
        });
    }

    private includesIgnoringDiacritics(text: string, substring: string) : boolean {
        const normalize = (str : string) =>
        str.normalize("NFD") // decompose accents
            .replace(/[\u0300-\u036f]/g, "") // remove accents
            .toLowerCase();
        return normalize(text).includes(normalize(substring));
    }
}
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../../Shared/BodyController";
import { watchdogScrapingResultsWebPagesInitializedEventName } from "../../ScrapingResults/ScrapingResultsController";
import Enumerable from "linq";
import { StimulusControllers } from "../../../Shared/Generated/StimulusControllers";
import BaseStimulusModelController from "../../../Shared/BaseStimulusModelController";
import { ScrapingResultsWebPagesStimulusModel } from "../../../Shared/Generated/ScrapingResultsWebPagesStimulusModel";

export const searchTermModifiedEventName = "searchTermModified";

export default class ScrapingResultsWebPagesController extends BaseStimulusModelController<ScrapingResultsWebPagesStimulusModel> {
    
    static targets = [
        "scrapingResult",
        "allResults",
        "noResults",
        "noResultsMessage",
        "webPage"
    ];
   
    declare scrapingResultTargets: HTMLElement[];
    declare allResultsTarget: HTMLElement;
    declare noResultsTarget: HTMLElement;
    declare noResultsMessageTarget: HTMLSpanElement;
    declare webPageTargets: HTMLElement[];

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
        let noResults = true;

        this.scrapingResultTargets.forEach(scrapingResult => {
            if (!scrapingResult.textContent) {
                throw new Error(`Scraping result (${scrapingResult}) does not contain any text.`);
            }
            if (searchTerm && !this.includesIgnoringDiacritics(scrapingResult.textContent, searchTerm)) {
                scrapingResult.style.display = "none";
            }
            else {
                scrapingResult.style.display = "";
                noResults = false;
            }
        });

        this.allResultsTarget.style.display = noResults ? "none" : "";
        this.noResultsTarget.style.display = noResults ? "" : "none";

        this.noResultsMessageTarget.innerHTML = searchTerm 
            ? this.modelValue.noScrapedResultsMatchingTheSearchTermAvailableResource.replace("{0}", `<i>${searchTerm}</i>`)
            : this.modelValue.noScrapedResultsAvailableResource;

        this.refreshWebPagesVisibility();
    }

    private includesIgnoringDiacritics(text: string, substring: string) : boolean {
        const normalize = (str : string) =>
        str.normalize("NFD") // decompose accents
            .replace(/[\u0300-\u036f]/g, "") // remove accents
            .toLowerCase();
        return normalize(text).includes(normalize(substring));
    }

    private refreshWebPagesVisibility() {
        this.webPageTargets.forEach(webPage => {
     
            const webPageScrapingResults = Enumerable.from(this.scrapingResultTargets).where(scrapingResult =>
                webPage.contains(scrapingResult)
            );

            const allHidden = Enumerable.from(webPageScrapingResults).all(x => x.style.display === "none");

            webPage.style.display = allHidden ? "none" : "";
        });
    }
}
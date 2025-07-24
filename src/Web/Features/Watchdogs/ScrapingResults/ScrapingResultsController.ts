import { Controller } from "@hotwired/stimulus";
import { ScrapingResultsConstants } from "../../Shared/Generated/ScrapingResultsConstants";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import Enumerable from "linq";
import { WatchdogConstants } from "../../Shared/Generated/WatchdogConstants";

export default class ScrapingResultsController extends Controller {
    static targets = [
        "searchTerm",
        "selectedElement",
        "createWatchdogAlert",
        "createWatchdogAlertForm"
    ];
   
    declare searchTermTarget: HTMLInputElement;
    declare selectedElementTargets: HTMLElement[];
    declare createWatchdogAlertTarget: HTMLButtonElement;
    declare createWatchdogAlertFormTarget: HTMLFormElement;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.createWatchdogAlertFormTarget, 
            async jobDto => {
                const watchdogAlertEntity = Enumerable.from(jobDto.affectedEntities)
                    .singleOrDefault(x => x.entityName === DomainConstants.watchdogAlertEntityName);
                if (!watchdogAlertEntity) {
                    throw new Error("Error getting WatchdogAlert.");
                }
                
                const watchdogAlertUrl = WatchdogConstants.watchdogAlertUrl
                    .replace(WatchdogConstants.watchdogAlertIdVariable, String(watchdogAlertEntity.entityId));
                Turbo.visit(watchdogAlertUrl);
            }
        );
    }

    onSearchTermModified() {
        let searchTerm = this.searchTermTarget.value;

        this.selectedElementTargets.forEach(selectedElement => {
            if (!selectedElement.textContent) {
                throw new Error("selectedElement does not contain any text."); // todo: make sure selected element always has text - when scraping the web page and selecting results, make sure there text and not just empty element
            }
            if (searchTerm && !this.includesIgnoringDiacritics(selectedElement.textContent, searchTerm)) {
                selectedElement.style.display = "none";
            }
            else {
                selectedElement.style.display = "list-item";
            }
        });

        this.createWatchdogAlertTarget.textContent = searchTerm 
            ? ScrapingResultsConstants.createAlertButtonSearchLabel
            : ScrapingResultsConstants.createAlertButtonDefaultLabel;
    }

    private includesIgnoringDiacritics(text: string, substring: string) : boolean {
        const normalize = (str : string) =>
        str.normalize("NFD") // decompose accents
            .replace(/[\u0300-\u036f]/g, "") // remove accents
            .toLowerCase();
        return normalize(text).includes(normalize(substring));
    }
}
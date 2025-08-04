import { Controller } from "@hotwired/stimulus";
import { ScrapingResultsConstants } from "../../Shared/Generated/ScrapingResultsConstants";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import Enumerable from "linq";
import { WatchdogConstants } from "../../Shared/Generated/WatchdogConstants";
import { searchTermModifiedEventName } from "../ScrapingResultsWebPages/ScrapingResultsWebPagesController";

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

        this.dispatch(searchTermModifiedEventName, { detail: searchTerm, prefix: "" });
        
        this.createWatchdogAlertTarget.textContent = searchTerm 
            ? ScrapingResultsConstants.createAlertButtonNewMatchingResultsLabel
            : ScrapingResultsConstants.createAlertButtonNewResultsLabel;
    }
}
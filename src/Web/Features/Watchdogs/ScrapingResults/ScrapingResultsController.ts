import { Controller } from "@hotwired/stimulus";
import { ScrapingResultsConstants } from "../../Shared/Generated/ScrapingResultsConstants";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import Enumerable from "linq";
import { WatchdogUrlConstants } from "../../Shared/Generated/WatchdogUrlConstants";
import { AccountUrlConstants } from "../../Shared/Generated/AccountUrlConstants";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../Shared/BodyController";
import { searchTermModifiedEventName } from "../Shared/ScrapingResultsWebPages/ScrapingResultsWebPagesController";

export const watchdogScrapingResultsWebPagesInitializedEventName = "watchdogScrapingResultsWebPagesInitializedEventName";

export default class ScrapingResultsController extends Controller {
    static targets = [
        "searchTerm",
        "createWatchdogAlert",
        "loginToCreateWatchdogAlert",
        "createWatchdogAlertForm",
    ];
   
    declare searchTermTarget: HTMLInputElement;
    declare createWatchdogAlertTarget: HTMLButtonElement;
    declare loginToCreateWatchdogAlertTarget: HTMLButtonElement;
    declare createWatchdogAlertFormTarget: HTMLFormElement;

    connect() {
        this.registerWatchdogScrapingResultsWebPagesInitializedEventHandler();

        formSubmitWithWaitForJobCompletion(
            this.createWatchdogAlertFormTarget, 
            async jobDto => {
                const watchdogAlertEntity = Enumerable.from(jobDto.affectedEntities)
                    .firstOrDefault(x => x.entityName === DomainConstants.watchdogAlertEntityName);
                if (!watchdogAlertEntity) {
                    throw new Error("Error getting WatchdogAlert.");
                }
                
                const watchdogAlertUrl = WatchdogUrlConstants.watchdogAlertUrlTemplate
                    .replace(WatchdogUrlConstants.watchdogAlertIdVariable, String(watchdogAlertEntity.entityId));
                Turbo.visit(watchdogAlertUrl);
            }
        );
    }

    onSearchTermModified() {
        let searchTerm = this.searchTermTarget.value;

        this.dispatch(searchTermModifiedEventName, { detail: searchTerm, prefix: "" });
        
        this.createWatchdogAlertTarget.innerHTML = searchTerm 
            ? ScrapingResultsConstants.createAlertButtonNewMatchingResultsLabelTemplate
                .replace(ScrapingResultsConstants.searchTermVariable, searchTerm)
            : ScrapingResultsConstants.createAlertButtonNewResultsLabel;

        this.loginToCreateWatchdogAlertTarget.innerHTML = searchTerm 
            ? ScrapingResultsConstants.loginOrRegisterToCreateAlertButtonNewMatchingResultsLabelTemplate
                .replace(ScrapingResultsConstants.searchTermVariable, searchTerm)
            : ScrapingResultsConstants.loginOrRegisterToCreateAlertButtonNewResultsLabel;
    }

    loginToAlertAboutNewResults() {
        let currentUrl = window.location.href;
        if (this.searchTermTarget.value) {
            const url = new URL(currentUrl);
            url.searchParams.set("searchTerm", this.searchTermTarget.value);
            currentUrl = url.toString();
        }
        const currentRelativeUrl = encodeURIComponent(currentUrl.replace(window.location.origin, ""));

        Turbo.visit(AccountUrlConstants.accountLoginUrlTemplate.replace(AccountUrlConstants.returnUrlVariable, currentRelativeUrl))
    }

    private registerWatchdogScrapingResultsWebPagesInitializedEventHandler() {
        this.dispatch(registerGlobalEventHandlerEventName, {
            prefix: "",
            detail: new EventHandlerRegistration<string>(
                watchdogScrapingResultsWebPagesInitializedEventName, 
                this.handleWatchdogScrapingResultsWebPagesInitializedEvent.bind(this)
            )
        });
    }

    private handleWatchdogScrapingResultsWebPagesInitializedEvent(event: CustomEventInit) {
        this.onSearchTermModified();
    }
}
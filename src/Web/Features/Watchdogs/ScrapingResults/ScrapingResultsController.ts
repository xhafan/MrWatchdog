import { Controller } from "@hotwired/stimulus";
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
        "createWatchdogSearch",
        "loginToCreateWatchdogSearch",
        "createWatchdogSearchForm",
    ];
   
    declare searchTermTarget: HTMLInputElement;
    declare createWatchdogSearchTarget: HTMLButtonElement;
    declare loginToCreateWatchdogSearchTarget: HTMLButtonElement;
    declare createWatchdogSearchFormTarget: HTMLFormElement;

    connect() {
        this.registerWatchdogScrapingResultsWebPagesInitializedEventHandler();

        formSubmitWithWaitForJobCompletion(
            this.createWatchdogSearchFormTarget, 
            async jobDto => {
                const watchdogSearchEntity = Enumerable.from(jobDto.affectedEntities)
                    .firstOrDefault(x => x.entityName === DomainConstants.watchdogSearchEntityName);
                if (!watchdogSearchEntity) {
                    throw new Error(`Error getting ${DomainConstants.watchdogSearchEntityName}.`);
                }
                
                const watchdogSearchUrl = WatchdogUrlConstants.watchdogSearchUrlTemplate
                    .replace(WatchdogUrlConstants.watchdogSearchIdVariable, String(watchdogSearchEntity.entityId));
                Turbo.visit(watchdogSearchUrl);
            }
        );
    }

    onSearchTermModified() {
        let searchTerm = this.searchTermTarget.value;

        this.dispatch(searchTermModifiedEventName, { detail: searchTerm, prefix: "" });        
    }

    loginToNotifyAboutNewResults() {
        let currentUrl = window.location.href;
        const url = new URL(currentUrl);
        if (this.searchTermTarget.value) {
            url.searchParams.set("searchTerm", this.searchTermTarget.value);
            currentUrl = url.toString();
        }
        const currentRelativeUrl = currentUrl.replace(window.location.origin, "");

        let accountLoginUrl = new URL(AccountUrlConstants.accountLoginUrl, window.location.origin);
        accountLoginUrl.searchParams.set(AccountUrlConstants.returnUrl, currentRelativeUrl);

        Turbo.visit(accountLoginUrl.toString());
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
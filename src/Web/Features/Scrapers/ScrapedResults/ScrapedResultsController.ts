import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import Enumerable from "linq";
import { WatchdogUrlConstants } from "../../Shared/Generated/WatchdogUrlConstants";
import { AccountUrlConstants } from "../../Shared/Generated/AccountUrlConstants";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../Shared/BodyController";
import { searchTermModifiedEventName } from "../Shared/ScrapedResultsWebPages/ScrapedResultsWebPagesController";

export const scraperScrapedResultsWebPagesInitializedEventName = "scraperScrapedResultsWebPagesInitialized";

export default class ScrapedResultsController extends Controller {
    static targets = [
        "searchTerm",
        "createWatchdog",
        "loginToCreateWatchdog",
        "createWatchdogForm",
    ];
   
    declare searchTermTarget: HTMLInputElement;
    declare createWatchdogTarget: HTMLButtonElement;
    declare loginToCreateWatchdogTarget: HTMLButtonElement;
    declare createWatchdogFormTarget: HTMLFormElement;

    connect() {
        this.registerScraperScrapedResultsWebPagesInitializedEventHandler();

        formSubmitWithWaitForJobCompletion(
            this.createWatchdogFormTarget, 
            async jobDto => {
                const watchdogEntity = Enumerable.from(jobDto.affectedEntities)
                    .firstOrDefault(x => x.entityName === DomainConstants.watchdogEntityName);
                if (!watchdogEntity) {
                    throw new Error(`Error getting ${DomainConstants.watchdogEntityName}.`);
                }
                
                const watchdogUrl = WatchdogUrlConstants.watchdogDetailUrlTemplate
                    .replace(WatchdogUrlConstants.watchdogIdVariable, String(watchdogEntity.entityId));
                Turbo.visit(watchdogUrl);
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

    private registerScraperScrapedResultsWebPagesInitializedEventHandler() {
        this.dispatch(registerGlobalEventHandlerEventName, {
            prefix: "",
            detail: new EventHandlerRegistration<string>(
                scraperScrapedResultsWebPagesInitializedEventName, 
                this.handleScraperScrapedResultsWebPagesInitializedEvent.bind(this)
            )
        });
    }

    private handleScraperScrapedResultsWebPagesInitializedEvent(event: CustomEventInit) {
        this.onSearchTermModified();
    }
}
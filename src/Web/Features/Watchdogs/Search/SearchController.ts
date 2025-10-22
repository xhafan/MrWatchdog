import { Controller } from "@hotwired/stimulus";
import { searchTermModifiedEventName } from "../Shared/ScrapingResultsWebPages/ScrapingResultsWebPagesController";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../Shared/BodyController";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import { WatchdogUrlConstants } from "../../Shared/Generated/WatchdogUrlConstants";

export default class SearchController extends Controller {
    static targets  = [
        "searchTermSuffix",
        "archiveWatchdogSearchForm"
    ];

    declare searchTermSuffixTarget: HTMLSpanElement;
    declare archiveWatchdogSearchFormTarget: HTMLFormElement;

    connect() {
        this.registerSearchTermModifiedEventHandler();

        formSubmitWithWaitForJobCompletion(
            this.archiveWatchdogSearchFormTarget, 
            async jobDto => {
                Turbo.visit(WatchdogUrlConstants.watchdogsSearchesUrl);
            },
            "Really delete this watchdog search?"
        );
    }

    private registerSearchTermModifiedEventHandler() {
        this.dispatch(registerGlobalEventHandlerEventName, {
            prefix: "",
            detail: new EventHandlerRegistration<string>(searchTermModifiedEventName, this.handleSearchTermModifiedEvent.bind(this))
        });
    }

    private handleSearchTermModifiedEvent(event: CustomEventInit<string>) {
        const searchTerm = event.detail;

        this.searchTermSuffixTarget.textContent = searchTerm ? ` - ${searchTerm}` : "";
    }
}

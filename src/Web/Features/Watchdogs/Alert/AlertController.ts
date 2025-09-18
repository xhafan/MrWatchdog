import { Controller } from "@hotwired/stimulus";
import { searchTermModifiedEventName } from "../Shared/ScrapingResultsWebPages/ScrapingResultsWebPagesController";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../Shared/BodyController";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import { WatchdogUrlConstants } from "../../Shared/Generated/WatchdogUrlConstants";

export default class AlertController extends Controller {
    static targets  = [
        "searchTermSuffix",
        "deleteWatchdogAlertForm"
    ];

    declare searchTermSuffixTarget: HTMLSpanElement;
    declare deleteWatchdogAlertFormTarget: HTMLFormElement;

    connect() {
        this.registerSearchTermModifiedEventHandler();

        formSubmitWithWaitForJobCompletion(
            this.deleteWatchdogAlertFormTarget, 
            async jobDto => {
                Turbo.visit(WatchdogUrlConstants.watchdogsAlertsUrl);
            },
            "Really delete the watchdog alert?"
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

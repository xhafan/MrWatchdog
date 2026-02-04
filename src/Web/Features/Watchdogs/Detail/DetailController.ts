import { Controller } from "@hotwired/stimulus";
import { searchTermModifiedEventName } from "../../Scrapers/Shared/ScrapedResultsWebPages/ScrapedResultsWebPagesController";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../Shared/BodyController";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import { WatchdogUrlConstants } from "../../Shared/Generated/WatchdogUrlConstants";
import BaseStimulusModelController from "../../Shared/BaseStimulusModelController";
import { WatchdogDetailStimulusModel } from "../../Shared/Generated/WatchdogDetailStimulusModel";

export default class DetailController extends BaseStimulusModelController<WatchdogDetailStimulusModel> {
    static targets  = [
        "searchTermSuffix",
        "archiveWatchdogForm"
    ];

    declare searchTermSuffixTarget: HTMLSpanElement;
    declare archiveWatchdogFormTarget: HTMLFormElement;

    connect() {
        this.registerSearchTermModifiedEventHandler();

        formSubmitWithWaitForJobCompletion(
            this.archiveWatchdogFormTarget, 
            async jobDto => {
                Turbo.visit(WatchdogUrlConstants.watchdogsUrl);
            },
            this.modelValue.deleteWatchdogConfirmationMessageResource
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

import { Controller } from "@hotwired/stimulus";
import { searchTermModifiedEventName } from "../ScrapingResultsWebPages/ScrapingResultsWebPagesController";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../Shared/BodyController";

export default class AlertController extends Controller {
    static targets  = [
        "searchTermPrefix"
    ];

    declare searchTermPrefixTarget: HTMLSpanElement;

    connect() {
        this.registerSearchTermModifiedEventHandler();
    }

    private registerSearchTermModifiedEventHandler() {
        this.dispatch(registerGlobalEventHandlerEventName, {
            prefix: "",
            detail: new EventHandlerRegistration<string>(searchTermModifiedEventName, this.handleSearchTermModifiedEvent.bind(this))
        });
    }

    private handleSearchTermModifiedEvent(event: CustomEventInit<string>) {
        const searchTerm = event.detail;

        this.searchTermPrefixTarget.textContent = searchTerm ? `${searchTerm} - ` : "";
    }
}
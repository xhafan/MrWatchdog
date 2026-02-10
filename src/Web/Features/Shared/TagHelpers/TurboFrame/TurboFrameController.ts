import { Controller } from "@hotwired/stimulus";
import { FrameElement } from "@hotwired/turbo";
import { formEditingCancelledEventName, formSubmitJobCompletedEventName } from "../ViewOrEditForm/ViewOrEditFormController";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../BodyController";

export default class TurboFrameController extends Controller {
    
    static values = {
        reloadOnEvent: String
    }

    declare reloadOnEventValue?: string;

    declare boundOnFormSubmitJobCompleted: (event: Event) => void;
    declare boundOnFormEditingCancelled: (event: Event) => void;

    connect() {
        this.attachEventListeners();

        if (this.reloadOnEventValue) {
            this.dispatch(registerGlobalEventHandlerEventName, {
                prefix: "",
                detail: new EventHandlerRegistration(this.reloadOnEventValue, () => {
                    this.reload();
                })
            });
        }
    }

    disconnect() {
        this.removeEventListeners();
    }

    private attachEventListeners() {
        this.boundOnFormSubmitJobCompleted = this.onFormSubmitJobCompleted.bind(this);
        this.boundOnFormEditingCancelled = this.onFormEditingCancelled.bind(this);
        
        this.element.addEventListener(formSubmitJobCompletedEventName, this.boundOnFormSubmitJobCompleted);
        this.element.addEventListener(formEditingCancelledEventName, this.boundOnFormEditingCancelled);
    }

    private removeEventListeners() {
        this.element.removeEventListener(formSubmitJobCompletedEventName, this.boundOnFormSubmitJobCompleted);
        this.element.removeEventListener(formEditingCancelledEventName, this.boundOnFormEditingCancelled);
    }

    private onFormSubmitJobCompleted(event: Event) {
        this.reload();
        event.stopPropagation();
    }

    private onFormEditingCancelled(event: Event) {
        this.reload();
        event.stopPropagation();
    }

    private reload() {
        (this.element as FrameElement).reload();
    }
}
import { Controller } from "@hotwired/stimulus";
import { FrameElement } from "@hotwired/turbo";
import { formEditingCancelledEventName, formSubmitJobCompletedEventName } from "../ViewOrEditForm/ViewOrEditFormController";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../BodyController";

export default class TurboFrameController extends Controller {

    connect() {
        this.attachEventListeners();
    }

    private attachEventListeners() {
        this.element.addEventListener(formSubmitJobCompletedEventName, this.onFormSubmitJobCompleted.bind(this), {});
        this.element.addEventListener(formEditingCancelledEventName, this.onFormEditingCancelled.bind(this), {});
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
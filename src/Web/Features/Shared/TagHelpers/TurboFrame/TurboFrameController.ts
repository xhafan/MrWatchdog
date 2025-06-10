import { Controller } from "@hotwired/stimulus";
import { FrameElement } from "@hotwired/turbo";
import { formEditingCancelledEventName, formSubmittedEventName } from "../ViewOrEditForm/ViewOrEditFormController";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../BodyController";

export default class TurboFrameController extends Controller {
    static targets = ["turboFrame"];

    declare turboFrameTarget: FrameElement;

    connect() {
        this.attachFormEventListeners();
    }

    private attachFormEventListeners() {
        this.turboFrameTarget.addEventListener(formSubmittedEventName, this.onFormSubmitted.bind(this), {});
        this.turboFrameTarget.addEventListener(formEditingCancelledEventName, this.onFormEditingCancelled.bind(this), {});
    }

    private onFormSubmitted() {
        this.reload();
    }

    private onFormEditingCancelled() {
        this.reload();
    }

    private reload() {
        this.turboFrameTarget.reload();
    }
}
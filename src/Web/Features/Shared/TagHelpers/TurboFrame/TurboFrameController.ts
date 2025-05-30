import { FrameElement } from "@hotwired/turbo";
import BaseStimulusModelController from "../../../BaseStimulusModelController";
import { TurboFrameStimulusModel } from "../../Generated/TurboFrameStimulusModel";
import { formEditingCancelledEventName, formSubmittedEventName } from "../ViewOrEditForm/ViewOrEditFormController";
import { EventHandlerRegistration, registerGlobalEventHandlerEventName } from "../../BodyController";

export default class TurboFrameController extends BaseStimulusModelController<TurboFrameStimulusModel> {
    static targets = ["turboFrame"];

    declare turboFrameTarget: FrameElement;

    connect() {
        this.attachFormEventListeners();

        if (!this.modelValue.reloadOnEvent) {
            this.dispatch(registerGlobalEventHandlerEventName, {
                prefix: "",
                detail: new EventHandlerRegistration(this.modelValue.reloadOnEvent, () => {
                    this.reload();
                })
            });
        }
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
import { Controller } from "@hotwired/stimulus";
import { removeOnboardingKeysFromLocalStorage } from "./TagHelpers/Onboarding/OnboardingController";
import { registerGlobalEventHandlerEventName, EventHandlerRegistration } from "../../../CoreWeb/Features/Shared/EventHandlerRegistration";

export default class BodyController extends Controller {
    connect() {
        this.element.addEventListener(registerGlobalEventHandlerEventName, this.registerGlobalEventHandler.bind(this), {});
    }

    registerGlobalEventHandler<T>(event: CustomEventInit<EventHandlerRegistration<T>>) {
        const eventHandlerRegistration = event.detail;
        if (!eventHandlerRegistration) throw new Error("Missing eventHandlerRegistration");
        this.element.addEventListener(
            eventHandlerRegistration.eventName,
            (event: CustomEventInit) => eventHandlerRegistration.eventHandler(event)
        );
    }

    logout() {
        removeOnboardingKeysFromLocalStorage();
    }
}
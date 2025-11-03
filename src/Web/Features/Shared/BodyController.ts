import { Controller } from "@hotwired/stimulus";
import { removeOnboardingKeysFromLocalStorage } from "./TagHelpers/Onboarding/OnboardingController";

export const registerGlobalEventHandlerEventName = "body:registerGlobalEventHandler";

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

export class EventHandlerRegistration<T> {
    eventName: string;
    eventHandler: (event: CustomEventInit<T>) => void;

    constructor(eventName: string, eventHandler: (event: CustomEventInit<T>) => void) {
        this.eventName = eventName;
        this.eventHandler = eventHandler;
    }
}
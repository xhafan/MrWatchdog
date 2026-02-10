import { Controller } from "@hotwired/stimulus";
import { removeOnboardingKeysFromLocalStorage } from "./TagHelpers/Onboarding/OnboardingController";

export const registerGlobalEventHandlerEventName = "body:registerGlobalEventHandler";

export default class BodyController extends Controller {
    declare boundRegisterGlobalEventHandler: (event: CustomEventInit<EventHandlerRegistration<any>>) => void;

    connect() {
        this.attachEventListeners();
    }

    disconnect() {
        this.removeEventListeners();
    }

    private attachEventListeners() {
        this.boundRegisterGlobalEventHandler = this.registerGlobalEventHandler.bind(this);
        this.element.addEventListener(registerGlobalEventHandlerEventName, this.boundRegisterGlobalEventHandler);
    }

    private removeEventListeners() {
        this.element.removeEventListener(registerGlobalEventHandlerEventName, this.boundRegisterGlobalEventHandler);
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
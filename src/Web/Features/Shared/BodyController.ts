import { Controller } from "@hotwired/stimulus";
import { removeOnboardingKeysFromLocalStorage } from "./TagHelpers/Onboarding/OnboardingController";

export const registerGlobalEventHandlerEventName = "body:registerGlobalEventHandler";

export default class BodyController extends Controller {
    declare boundRegisterGlobalEventHandler: (event: CustomEventInit<EventHandlerRegistration<any>>) => void;
    private registeredEventListenersByEventName: Map<string, EventListener[]> = new Map();

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
        
        this.registeredEventListenersByEventName.forEach((listeners, eventName) => {
            listeners.forEach(listener => {
                this.element.removeEventListener(eventName, listener);
            });
        });
        this.registeredEventListenersByEventName.clear();
    }
    
    registerGlobalEventHandler<T>(event: CustomEventInit<EventHandlerRegistration<T>>) {
        const eventHandlerRegistration = event.detail;
        if (!eventHandlerRegistration) throw new Error("Missing eventHandlerRegistration");
        
        const boundHandler = (event: CustomEventInit) => eventHandlerRegistration.eventHandler(event);
        this.element.addEventListener(eventHandlerRegistration.eventName, boundHandler);
        
        const existingListeners = this.registeredEventListenersByEventName.get(eventHandlerRegistration.eventName) || [];
        existingListeners.push(boundHandler);
        this.registeredEventListenersByEventName.set(eventHandlerRegistration.eventName, existingListeners);
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
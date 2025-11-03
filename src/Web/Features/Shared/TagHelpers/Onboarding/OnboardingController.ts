import BaseStimulusModelController from "../../BaseStimulusModelController";
import { OnboardingStimulusModel } from "../../Generated/OnboardingStimulusModel";
import "@sjmc11/tourguidejs/dist/css/tour.min.css";
import {TourGuideClient} from "@sjmc11/tourguidejs";
import { UserUrlConstants } from "../../Generated/UserUrlConstants";

const onboardingLocalStorageKeyPrefix = "onboardingComplete_";

export function removeOnboardingKeysFromLocalStorage() {
    for (const key in localStorage) {
        if (key.startsWith(onboardingLocalStorageKeyPrefix)) {
            localStorage.removeItem(key);
        }
    }
}

export default class OnboardingController extends BaseStimulusModelController<OnboardingStimulusModel>  {
   
    isDisconnected: boolean = false;
    
    tourGuide: TourGuideClient | undefined;
    turboBeforeCacheHandler: (() => void) | undefined;
    onboardingLocalStorageKey: string | undefined;

    connect() {
        this.turboBeforeCacheHandler = this.clearTourGuideDom.bind(this);
        document.addEventListener("turbo:before-cache", this.turboBeforeCacheHandler);
        
        this.onboardingLocalStorageKey = `${onboardingLocalStorageKeyPrefix}${this.modelValue.onboardingIdentifier}`;

        this.initializeOnboarding();
    }

    initializeOnboarding() {
        this.tourGuide = new TourGuideClient({
            backdropColor: "rgba(20,20,21,0.5)",
            steps: [], // this is needed for navigation to a Turbo cached page to prevent seeing old steps
            exitOnClickOutside: false
        });

        this.tourGuide.onAfterExit(async () => {
            if (!this.isDisconnected) { // checking this.isDisconnected is needed so the last Turbo cached version of the tourGuide doesn't call this after disconnect
                await this.markOnboardingAsComplete();
            }
        });

        this.modelValue.steps.forEach((step, i) => {
            
            if (step.elementIdentifier) {
                var element = document.querySelector(step.elementIdentifier);
                if (!element) return;
    
                // @ts-ignore
                var isElementVisible = element.offsetParent !== null; // more info https://stackoverflow.com/a/21696585/379279s
                if (!isElementVisible) return;
            }

            this.tourGuide!.addSteps([{
                title: step.title,
                content: step.text,
                target: step.elementIdentifier
            }]);
        });

        if (this.tourGuide.tourSteps.length === 0 
            || !this.modelValue.enableOnboarding) return;

        if (!this.isOnboardingComplete()) {
            this.startTourGuide();
        }
    }

    private startTourGuide() {
        if (this.tourGuide?.isVisible) return;

        this.tourGuide!.start();
    }

    async disconnect() {
        document.removeEventListener("turbo:before-cache", this.turboBeforeCacheHandler!);
        this.tourGuide = undefined; // this prevents previously cached tour guide onAfterExit handlers from being called
        this.isDisconnected = true;
    }

    clearTourGuideDom() { 
        // this is needed for navigation to a Turbo cached page to prevent seeing old tour guide onboarding dialog
        document.querySelectorAll(".tg-backdrop, .tg-dialog").forEach(el => el.remove());
    }

    showOnboarding() {
        this.startTourGuide();
    }

    private async markOnboardingAsComplete() {
        this.markOnboardingAsCompleteInTheLocalStorage();
        await this.markOnboardingAsCompleteInTheBackend();
    }

    private markOnboardingAsCompleteInTheLocalStorage() {
        if (!this.onboardingLocalStorageKey) throw new Error(`${this.onboardingLocalStorageKey} is undefined`);
        localStorage.setItem(this.onboardingLocalStorageKey, "true");
    }

    private async markOnboardingAsCompleteInTheBackend() {
        if (!this.modelValue.isUserAuthenticated) return

        var completeOnboardingUrl = UserUrlConstants.apiCompleteOnboardingUrlTemplate
            .replace(UserUrlConstants.onboardingIdentifierVariable, this.modelValue.onboardingIdentifier);
        const completeOnboardingResponse = await fetch(completeOnboardingUrl, { method: "POST" });
        if (!completeOnboardingResponse.ok) {
            throw new Error(`Error completing onboarding ${this.modelValue.onboardingIdentifier}: HTTP ${completeOnboardingResponse.status}`);
        }
    }

    private isOnboardingComplete() {
        if (!this.onboardingLocalStorageKey) throw new Error(`${this.onboardingLocalStorageKey} is undefined`);

        return localStorage.getItem(this.onboardingLocalStorageKey) === "true";
    }
}

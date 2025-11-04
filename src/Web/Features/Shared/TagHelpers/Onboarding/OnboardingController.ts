import BaseStimulusModelController from "../../BaseStimulusModelController";
import { OnboardingStimulusModel } from "../../Generated/OnboardingStimulusModel";
import "@sjmc11/tourguidejs/dist/css/tour.min.css";
import {TourGuideClient} from "@sjmc11/tourguidejs";
import { UserUrlConstants } from "../../Generated/UserUrlConstants";
import { FrameElement } from "@hotwired/turbo";
import Enumerable from "linq";

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

    async connect() {
        this.turboBeforeCacheHandler = this.clearPreviousInstanceOfTourGuide.bind(this);
        document.addEventListener("turbo:before-cache", this.turboBeforeCacheHandler);

        this.onboardingLocalStorageKey = `${onboardingLocalStorageKeyPrefix}${this.modelValue.onboardingIdentifier}`;

        if (this.modelValue.enableOnboarding && !this.isOnboardingComplete()) {
            await this.startOnboarding();
        }
    }

    async startOnboarding() {
        if (this.tourGuide?.isVisible) return;

        this.clearPreviousInstanceOfTourGuide();
        await this.waitForAllTurboFramesLoaded();

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
            
            let visibleElement;

            if (step.elementIdentifier) {
                var elements = Enumerable.from(document.querySelectorAll(step.elementIdentifier));
                if (elements.count() == 0) return;
    
                // @ts-ignore
                visibleElement = elements.firstOrDefault(x => x.offsetParent !== null); // more info https://stackoverflow.com/a/21696585/379279s                
                if (!visibleElement) return;
            }

            this.tourGuide!.addSteps([{
                title: step.title,
                content: step.text,
                target: visibleElement
            }]);
        });

        await this.tourGuide!.start();
    }

    async disconnect() {
        document.removeEventListener("turbo:before-cache", this.turboBeforeCacheHandler!);
        this.isDisconnected = true;
    }

    clearPreviousInstanceOfTourGuide() { 
        // this is needed for navigation to a Turbo cached page to prevent seeing old tour guide onboarding dialog
        document.querySelectorAll(".tg-backdrop, .tg-dialog").forEach(el => el.remove());
        this.tourGuide = undefined;
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

    private async waitForAllTurboFramesLoaded() {
        const frames : FrameElement[] = Array.from(document.querySelectorAll("turbo-frame[src]"));
        if (frames.length === 0) return;

        await Promise.all(frames.map(frame => frame.loaded));
    }
}

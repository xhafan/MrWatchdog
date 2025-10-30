import BaseStimulusModelController from "../../BaseStimulusModelController";
import { OnboardingStimulusModel } from "../../Generated/OnboardingStimulusModel";
import "@sjmc11/tourguidejs/dist/css/tour.min.css";
import {TourGuideClient} from "@sjmc11/tourguidejs";

export default class OnboardingController extends BaseStimulusModelController<OnboardingStimulusModel>  {
    
    tourGuide: TourGuideClient | undefined;
    turboBeforeCacheHandler: (() => void) | undefined;
    onboardingLocalStorageKey: string | undefined;

    connect() {
        this.turboBeforeCacheHandler = this.clearTourGuideDom.bind(this);
        document.addEventListener("turbo:before-cache", this.turboBeforeCacheHandler);
        
        this.onboardingLocalStorageKey = `onboardingComplete_${this.modelValue.onboardingIdentifier}`;

        this.initializeOnboarding();
    }

    initializeOnboarding() {
        this.tourGuide = new TourGuideClient({
            backdropColor: "rgba(20,20,21,0.5)",
            steps: [] // this is needed for navigation to a Turbo cached page to prevent seeing old steps
        });

        this.tourGuide.onAfterExit(() => {            
            this.markOnboardingAsComplete();
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
            this.tourGuide.start();
        }
    }

    async disconnect() {
        if (this.tourGuide) {
            await this.tourGuide.exit();
        }

        document.removeEventListener("turbo:before-cache", this.turboBeforeCacheHandler!);
    }

    clearTourGuideDom() { 
        // this is needed for navigation to a Turbo cached page to prevent seeing old tour guide onboarding dialog
        document.querySelectorAll(".tg-backdrop, .tg-dialog").forEach(el => el.remove());
    }

    showOnboarding() {
        if (this.tourGuide?.isVisible) return
        
        this.tourGuide!.start();
    }

    private markOnboardingAsComplete() {
        if (!this.onboardingLocalStorageKey) throw new Error(`${this.onboardingLocalStorageKey} is undefined`);

        localStorage.setItem(this.onboardingLocalStorageKey, "true");
    }

    private isOnboardingComplete() {
        if (!this.onboardingLocalStorageKey) throw new Error(`${this.onboardingLocalStorageKey} is undefined`);

        return localStorage.getItem(this.onboardingLocalStorageKey) === "true";
    }
}

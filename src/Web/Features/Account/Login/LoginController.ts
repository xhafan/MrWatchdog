import BaseStimulusModelController from "../../Shared/BaseStimulusModelController";
import { LoginStimulusModel } from "../../Shared/Generated/LoginStimulusModel";
import { disableElementAndAddSpinner } from "../../Jobs/jobCompletion";

export default class LoginController extends BaseStimulusModelController<LoginStimulusModel>  {
    static targets = [
        "form"
    ];

    declare formTarget: HTMLFormElement;
    
    declare boundFormSubmitHandler: (event: SubmitEvent) => void;

    connect() {
        this.attachEventListeners();
    }

    disconnect() {
        this.removeEventListeners();
    }

    private attachEventListeners() {
        this.boundFormSubmitHandler = this.handleFormSubmit.bind(this);
        this.formTarget.addEventListener("submit", this.boundFormSubmitHandler);
    }

    private removeEventListeners() {
        this.formTarget.removeEventListener("submit", this.boundFormSubmitHandler);
    }

    private async handleFormSubmit(event: SubmitEvent) {
        event.preventDefault();

        if (!$(this.formTarget).valid()) {
            return; 
        }
        
        disableElementAndAddSpinner(event.submitter);

        // @ts-ignore
        const grecaptcha = window.grecaptcha;

        if (!grecaptcha) {
            throw new Error("grecaptcha is not defined");
        }
        
        // wait until grecaptcha is ready
        await new Promise<void>((resolve) => grecaptcha.ready(resolve));
        
        const token: string = await grecaptcha.execute(this.modelValue.reCaptchaSiteKey, {action: "submit"});

        const recaptchaResponseInputElement = document.getElementById("g-recaptcha-response-1") as HTMLInputElement | null;
        if (!recaptchaResponseInputElement) throw new Error("ReCaptcha response input element not found");
        recaptchaResponseInputElement.value = token;
        
        this.formTarget.submit();            
    }
}
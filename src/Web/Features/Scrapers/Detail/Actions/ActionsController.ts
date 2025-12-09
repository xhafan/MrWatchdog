import { formSubmitWithWaitForJobCompletion } from "../../../Jobs/jobCompletion";
import { DomEvents } from "../../../Shared/Generated/DomEvents";
import BaseStimulusModelController from "../../../Shared/BaseStimulusModelController";
import { ActionsStimulusModel } from "../../../Shared/Generated/ActionsStimulusModel";

export default class ActionsControllers extends BaseStimulusModelController<ActionsStimulusModel> {
    static targets = [
        "requestToMakePublicForm",
        "makePublicForm",
        "makePrivateForm"
    ];
   
    declare requestToMakePublicFormTarget: HTMLFormElement;
    declare makePublicFormTarget: HTMLFormElement;
    declare makePrivateFormTarget: HTMLFormElement;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.requestToMakePublicFormTarget, 
            async jobDto => {
                this.dispatchScraperPublicStatusUpdated();
            },
            this.modelValue.requestToMakeScraperPublicConfirmationMessageResource
        );

        formSubmitWithWaitForJobCompletion(
            this.makePublicFormTarget,
            async jobDto => {
                this.dispatchScraperPublicStatusUpdated();
            },
            this.modelValue.makeScraperPublicConfirmationMessageResource
        );

        formSubmitWithWaitForJobCompletion(
            this.makePrivateFormTarget,
            async jobDto => {
                this.dispatchScraperPublicStatusUpdated();
            },
            `<i class="fa-solid fa-triangle-exclamation text-warning"></i> ${this.modelValue.makeScraperPrivateConfirmationMessageResource}`
        );
    }

    dispatchScraperPublicStatusUpdated() {
        this.dispatch(DomEvents.scraperPublicStatusUpdated);
    }
}
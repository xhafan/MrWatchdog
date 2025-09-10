import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../../Jobs/jobCompletion";
import { DomEvents } from "../../../Shared/Generated/DomEvents";

export default class ActionsControllers extends Controller {
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
                this.dispatchWatchdogPublicStatusUpdated();
            },
            "Really request to make the watchdog public and discoverable by other users?"
        );

        formSubmitWithWaitForJobCompletion(
            this.makePublicFormTarget,
            async jobDto => {
                this.dispatchWatchdogPublicStatusUpdated();
            },
            "Really make the watchdog public and discoverable by other users?"
        );

        formSubmitWithWaitForJobCompletion(
            this.makePrivateFormTarget,
            async jobDto => {
                this.dispatchWatchdogPublicStatusUpdated();
            },
            '<i class="fa-solid fa-triangle-exclamation text-warning"></i> Really make the watchdog private and non-discoverable by other users?'
        );
    }

    dispatchWatchdogPublicStatusUpdated() {
        this.dispatch(DomEvents.watchdogPublicStatusUpdated);
    }
}
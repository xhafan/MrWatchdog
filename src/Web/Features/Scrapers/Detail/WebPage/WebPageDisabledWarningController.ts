import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../../Jobs/jobCompletion";

export default class WebPageDisabledWarningController extends Controller {
    static targets = [
        "enableWatchdogWebPageForm"
    ];
   
    declare enableWatchdogWebPageFormTarget: HTMLFormElement;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.enableWatchdogWebPageFormTarget,
            async jobDto => {
                const turboFrame = this.element.closest("turbo-frame");
                turboFrame?.reload();
            }
        );        
    }
}
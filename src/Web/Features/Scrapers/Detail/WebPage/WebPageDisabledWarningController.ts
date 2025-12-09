import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../../Jobs/jobCompletion";

export default class WebPageDisabledWarningController extends Controller {
    static targets = [
        "enableScraperWebPageForm"
    ];
   
    declare enableScraperWebPageFormTarget: HTMLFormElement;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.enableScraperWebPageFormTarget,
            async jobDto => {
                const turboFrame = this.element.closest("turbo-frame");
                turboFrame?.reload();
            }
        );        
    }
}
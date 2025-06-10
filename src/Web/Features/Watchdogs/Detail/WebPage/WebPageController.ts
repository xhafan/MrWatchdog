import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../../Jobs/jobCompletion";
import BaseStimulusModelController from "../../../Shared/BaseStimulusModelController";
import { WebPageStimulusModel } from "../../../Shared/Generated/WebPageStimulusModel";

export const watchdogWebPageRemovedEvent = "watchdogWebPageRemoved";

export default class WebPageController extends BaseStimulusModelController<WebPageStimulusModel> {
    static targets = [
        "url",
        "previousUrl",
        "name",
        "removeWebPageForm"
    ];
   
    declare urlTarget: HTMLInputElement;
    declare previousUrlTarget: HTMLInputElement;
    declare nameTarget: HTMLInputElement;
    declare removeWebPageFormTarget: HTMLFormElement;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.removeWebPageFormTarget, 
            async jobDto => {
                this.element.dispatchEvent(new CustomEvent(watchdogWebPageRemovedEvent, { bubbles: true, detail: this.element }));
            },
            "Really remove the web page to monitor?"
        );

        if (this.modelValue.isEmptyWebPage) {
            requestAnimationFrame(() => {
                this.urlTarget.scrollIntoView({ behavior: "smooth", block: "center" });
                this.urlTarget.focus();                
            });
        }
    }

    onUrlModified(event: InputEvent) {
        let url = this.urlTarget.value.trim();
        let urlWithoutHttpPrefix = this.getUrlWithoutHttpPrefix(url);
        let previousUrlWithoutHttpPrefix = this.getUrlWithoutHttpPrefix(this.previousUrlTarget.value);
        if (!this.nameTarget.value || this.nameTarget.value === previousUrlWithoutHttpPrefix) {
            this.nameTarget.value = urlWithoutHttpPrefix;
        }
        
        this.previousUrlTarget.value = url;
    }

    private getUrlWithoutHttpPrefix(url: string): string {
        return url.replace(/^https?:\/\//i, "");
    }    
}
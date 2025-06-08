import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../../Jobs/jobCompletion";

export const watchdogWebPageRemovedEvent = "watchdogWebPageRemoved";

export default class WebPageController extends Controller {
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
    }

    onUrlModified(event: InputEvent) {
        let url = this.urlTarget.value.trim();
        let cleanUrl = this.getCleanUrl(url);
        let previousCleanUrl = this.getCleanUrl(this.previousUrlTarget.value);
        if (!this.nameTarget.value || this.nameTarget.value === previousCleanUrl) {
            this.nameTarget.value = cleanUrl;
        }
        
        this.previousUrlTarget.value = url;
    }

    private getCleanUrl(url: string): string {
        return url.replace(/^https?:\/\//i, "");
    }    
}
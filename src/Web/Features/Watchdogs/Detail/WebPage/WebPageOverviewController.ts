import { Controller } from "@hotwired/stimulus";
import { FrameElement } from "@hotwired/turbo";
import { formSubmitWithWaitForJobCompletion, getRelatedDomainEventJobGuid, waitForJobCompletion } from "../../../Jobs/jobCompletion";
import BaseStimulusModelController from "../../../Shared/BaseStimulusModelController";
import { WebPageOverviewStimulusModel } from "../../../Shared/Generated/WebPageOverviewStimulusModel";
import { formSubmitJobCompletedEventName } from "../../../Shared/TagHelpers/ViewOrEditForm/ViewOrEditFormController";
import { JobDto } from "../../../Shared/Generated/JobDto";
import { JobConstants } from "../../../Shared/Generated/JobConstants";
import { DomainConstants } from "../../../Shared/Generated/DomainConstants";

export const watchdogWebPageNameModifiedEventName = "watchdogwebPageNameModified";

export default class WebPageOverviewController extends BaseStimulusModelController<WebPageOverviewStimulusModel> {
    static targets = [
        "url",
        "previousUrl",
        "name"
    ];
   
    declare urlTarget: HTMLInputElement;
    declare previousUrlTarget: HTMLInputElement;
    declare nameTarget: HTMLInputElement;

    connect() {
        if (this.modelValue.isEmptyWebPage) {
            requestAnimationFrame(() => {
                this.urlTarget.scrollIntoView({ behavior: "smooth", block: "center" });
                this.urlTarget.focus();                
            });
        }
    }

    onUrlModified() {
        let url = this.urlTarget.value.trim();
        let urlWithoutHttpPrefix = this.getUrlWithoutHttpPrefix(url);
        let previousUrlWithoutHttpPrefix = this.getUrlWithoutHttpPrefix(this.previousUrlTarget.value);
        if (!this.nameTarget.value || this.nameTarget.value === previousUrlWithoutHttpPrefix) {
            this.nameTarget.value = urlWithoutHttpPrefix;
            this.dispatchWatchdogWebPageNameModifiedEvent();
        }
        
        this.previousUrlTarget.value = url;
    }

    private getUrlWithoutHttpPrefix(url: string): string {
        return url.replace(/^https?:\/\//i, "");
    }

    onNameModified() {
        this.dispatchWatchdogWebPageNameModifiedEvent();
    }

    private dispatchWatchdogWebPageNameModifiedEvent() {
        let name = this.nameTarget.value.trim();
        this.nameTarget.dispatchEvent(new CustomEvent(watchdogWebPageNameModifiedEventName, { bubbles: true, detail: name }));
    }
}
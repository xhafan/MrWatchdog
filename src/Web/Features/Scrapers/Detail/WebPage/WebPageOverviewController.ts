import { Controller } from "@hotwired/stimulus";
import { FrameElement } from "@hotwired/turbo";
import { formSubmitWithWaitForJobCompletion, waitForJobCompletion } from "../../../Jobs/jobCompletion";
import BaseStimulusModelController from "../../../Shared/BaseStimulusModelController";
import { WebPageOverviewStimulusModel } from "../../../Shared/Generated/WebPageOverviewStimulusModel";
import { formSubmitJobCompletedEventName } from "../../../Shared/TagHelpers/ViewOrEditForm/ViewOrEditFormController";
import { JobDto } from "../../../Shared/Generated/JobDto";
import { JobUrlConstants } from "../../../Shared/Generated/JobUrlConstants";
import { DomainConstants } from "../../../Shared/Generated/DomainConstants";
import { ValidationConstants } from "../../../Shared/Generated/ValidationConstants";

export const scraperWebPageNameModifiedEventName = "scraperWebPageNameModified";

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
            this.nameTarget.value = urlWithoutHttpPrefix.substring(0, ValidationConstants.scraperWebPageNameMaxLength).trim();
            this.dispatchScraperWebPageNameModifiedEvent();
        }
        
        this.previousUrlTarget.value = url;
    }

    private getUrlWithoutHttpPrefix(url: string): string {
        return url.replace(/^https?:\/\//i, "");
    }

    onNameModified() {
        this.dispatchScraperWebPageNameModifiedEvent();
    }

    private dispatchScraperWebPageNameModifiedEvent() {
        let name = this.nameTarget.value.trim();
        this.nameTarget.dispatchEvent(new CustomEvent(scraperWebPageNameModifiedEventName, { bubbles: true, detail: name }));
    }
}
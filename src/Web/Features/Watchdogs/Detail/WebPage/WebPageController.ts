import { Controller } from "@hotwired/stimulus";
import { FrameElement } from "@hotwired/turbo";
import { formSubmitWithWaitForJobCompletion, getRelatedDomainEventJobGuid, waitForJobCompletion } from "../../../Jobs/jobCompletion";
import BaseStimulusModelController from "../../../Shared/BaseStimulusModelController";
import { WebPageStimulusModel } from "../../../Shared/Generated/WebPageStimulusModel";
import { formSubmitJobCompletedEventName } from "../../../Shared/TagHelpers/ViewOrEditForm/ViewOrEditFormController";
import { JobDto } from "../../../Shared/Generated/JobDto";
import { JobConstants } from "../../../Shared/Generated/JobConstants";
import { DomainConstants } from "../../../Shared/Generated/DomainConstants";


export const watchdogWebPageRemovedEvent = "watchdogWebPageRemoved";

export default class WebPageController extends BaseStimulusModelController<WebPageStimulusModel> {
    static targets = [
        "url",
        "previousUrl",
        "name",
        "removeWebPageForm",
        "viewOrEditWebPageForm",
        "webPageSelectedHtml"
    ];
   
    declare urlTarget: HTMLInputElement;
    declare previousUrlTarget: HTMLInputElement;
    declare nameTarget: HTMLInputElement;
    declare removeWebPageFormTarget: HTMLFormElement;
    declare viewOrEditWebPageFormTarget: HTMLFormElement;
    declare webPageSelectedHtmlTarget: FrameElement;

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

        this.viewOrEditWebPageFormTarget.addEventListener(formSubmitJobCompletedEventName, this.onUpdateWatchdogWebPageJobCompleted.bind(this), {});
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
    
    private async onUpdateWatchdogWebPageJobCompleted(event: CustomEventInit) {
        let updateWatchdogWebPageJobDto = event.detail as JobDto;
        let updateWatchdogWebPageJobGuid = updateWatchdogWebPageJobDto.guid;

        var watchdogWebPageUpdatedDomainEventJobGuid = await getRelatedDomainEventJobGuid(updateWatchdogWebPageJobGuid, DomainConstants.watchdogWebPageUpdatedDomainEvent);
        
        if (!watchdogWebPageUpdatedDomainEventJobGuid) return;

        var watchdogWebPageUpdatedDomainEventJobDto = await waitForJobCompletion(watchdogWebPageUpdatedDomainEventJobGuid);

        this.webPageSelectedHtmlTarget.reload();
    }
}
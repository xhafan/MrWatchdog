import { Controller } from "@hotwired/stimulus";
import BaseStimulusModelController from "../../Shared/BaseStimulusModelController";
import { ScraperDetailStimulusModel } from "../../Shared/Generated/ScraperDetailStimulusModel";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import Enumerable from "linq";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import { scraperWebPageRemovedEvent } from "./WebPage/WebPageController";
import { ScraperUrlConstants } from "../../Shared/Generated/ScraperUrlConstants";

export default class DetailController extends BaseStimulusModelController<ScraperDetailStimulusModel> {
    static targets = [
        "webPages",
        "addWebPageForm",
        "webPageTurboFrame",
        "archiveScraperForm"
    ];
   
    declare webPagesTarget: HTMLDivElement;
    declare addWebPageFormTarget: HTMLFormElement;
    declare webPageTurboFrameTargets: HTMLFormElement[];
    declare archiveScraperFormTarget: HTMLFormElement;

    connect() {
        formSubmitWithWaitForJobCompletion(
            this.addWebPageFormTarget, 
            async jobDto => {
                const scraperWebPageEntity = Enumerable.from(jobDto.affectedEntities)
                    .singleOrDefault(x => x.entityName === DomainConstants.scraperWebPageEntityName && x.isCreated);
                if (!scraperWebPageEntity) {
                    throw new Error(`Error getting created ${DomainConstants.scraperWebPageEntityName}.`);
                }

                const webPageTurboFrameUrl = ScraperUrlConstants.scraperDetailWebPageTurboFrameUrlTemplate
                    .replace(ScraperUrlConstants.scraperIdVariable, String(this.modelValue.scraperId))
                    .replace(ScraperUrlConstants.scraperWebPageIdVariable, String(scraperWebPageEntity.entityId));

                const response = await fetch(webPageTurboFrameUrl);
                if (response.ok) {
                    const html = await response.text();
                    this.webPagesTarget.insertAdjacentHTML("beforeend", html);
                }
            }
        );

        formSubmitWithWaitForJobCompletion(
            this.archiveScraperFormTarget, 
            async jobDto => {
                Turbo.visit(ScraperUrlConstants.scrapersManageUrl);
            },
            this.modelValue.deleteScraperConfirmationMessageResource
        );

        this.element.addEventListener(scraperWebPageRemovedEvent, this.onScraperWebPageRemoved.bind(this));
    }

    private onScraperWebPageRemoved(event: CustomEventInit) {
        let scraperWebPageElement = event.detail as HTMLElement;
     
        const parentTurboFrame = this.webPageTurboFrameTargets.find(
            webPageTurboFrame => webPageTurboFrame.contains(scraperWebPageElement)
        );

        if (!parentTurboFrame) throw new Error("Parent turbo-frame not found for the removed scraper web page.");
        parentTurboFrame.remove();
    }
}
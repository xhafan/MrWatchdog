import { Controller } from "@hotwired/stimulus";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import Enumerable from "linq";
import { ScraperUrlConstants } from "../../Shared/Generated/ScraperUrlConstants";

export default class CreateController extends Controller {
    static targets  = [
        "form"
    ];

    declare formTarget: HTMLFormElement;

    connect() {
        const form = this.formTarget;

        formSubmitWithWaitForJobCompletion(
            form, 
            jobDto => {
                const scraperEntity = Enumerable.from(jobDto.affectedEntities).singleOrDefault(x => x.entityName === DomainConstants.scraperEntityName);
                if (!scraperEntity) {
                    throw new Error(`Error getting created ${DomainConstants.scraperEntityName}.`);
                }

                const scraperDetailUrl = ScraperUrlConstants.scraperDetailUrlTemplate
                    .replace(ScraperUrlConstants.scraperIdVariable, String(scraperEntity.entityId));

                Turbo.visit(scraperDetailUrl);
            }
        );
    }
}
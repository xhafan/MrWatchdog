import { Controller } from "@hotwired/stimulus";
import { JobDto } from "../../Shared/Generated/JobDto";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import BaseStimulusModelController from "../../Shared/BaseStimulusModelController";
import { CreateStimulusModel } from "../../Shared/Generated/CreateStimulusModel";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import Enumerable from "linq";

export default class CreateController extends BaseStimulusModelController<CreateStimulusModel> {
    static targets  = [
        "form"
    ];

    declare formTarget: HTMLFormElement;

    connect() {
        const form = this.formTarget;

        formSubmitWithWaitForJobCompletion(
            form, 
            jobDto => {
                const watchdogEntity = Enumerable.from(jobDto.affectedEntities).singleOrDefault(x => x.entityName === DomainConstants.watchdog);
                if (!watchdogEntity) {
                    throw new Error("Error getting created Watchdog.");
                }

                const watchdogDetailUrl = this.modelValue.watchdogDetailUrl.replace("$id", String(watchdogEntity.entityId));

                Turbo.visit(watchdogDetailUrl);
            }
        );
    }
}
import { Controller } from "@hotwired/stimulus";
import { JobDto } from "../Shared/Generated/JobDto";
import { DomainConstants } from "../Shared/Generated/DomainConstants";
import BaseStimulusModelController from "../BaseStimulusModelController";
import { CreateStimulusModel } from "../Shared/Generated/CreateStimulusModel";
import { formSubmitWithWaitForJobCompletion } from "../Jobs/jobCompletion";

export default class CreateController extends BaseStimulusModelController<CreateStimulusModel> {
    static targets  = [
        "form"
    ];

    declare formTarget: HTMLFormElement;

    connect() {
        const form = this.formTarget;

        formSubmitWithWaitForJobCompletion(form, 
            this.modelValue.getJobUrl, 
            jobDto => {
                const watchdogEntity = jobDto.affectedAggregateRootEntities.find(e => e.aggregateRootEntityName === DomainConstants.watchdog);
                if (!watchdogEntity) {
                    throw new Error("Error getting created Watchdog Id.");
                }

                const watchdogDetailUrl = this.modelValue.watchdogDetailUrl.replace("$id", String(watchdogEntity.aggregateRootEntityId));

                Turbo.visit(watchdogDetailUrl);
            }
        );
    }
}
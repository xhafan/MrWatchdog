import { Controller } from "@hotwired/stimulus";
import { JobDto } from "../../Shared/Generated/JobDto";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import BaseStimulusModelController from "../../Shared/BaseStimulusModelController";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import Enumerable from "linq";
import { WatchdogConstants } from "../../Shared/Generated/WatchdogConstants";

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
                const watchdogEntity = Enumerable.from(jobDto.affectedEntities).singleOrDefault(x => x.entityName === DomainConstants.watchdogEntityName);
                if (!watchdogEntity) {
                    throw new Error("Error getting created Watchdog.");
                }

                const watchdogDetailUrl = WatchdogConstants.watchdogDetailUrl.replace(WatchdogConstants.watchdogIdVariable, String(watchdogEntity.entityId));

                Turbo.visit(watchdogDetailUrl);
            }
        );
    }
}
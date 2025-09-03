import { Controller } from "@hotwired/stimulus";
import { DomainConstants } from "../../Shared/Generated/DomainConstants";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import Enumerable from "linq";
import { WatchdogUrlConstants } from "../../Shared/Generated/WatchdogUrlConstants";

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

                const watchdogDetailUrl = WatchdogUrlConstants.watchdogDetailUrlTemplate.replace(WatchdogUrlConstants.watchdogIdVariable, String(watchdogEntity.entityId));

                Turbo.visit(watchdogDetailUrl);
            }
        );
    }
}
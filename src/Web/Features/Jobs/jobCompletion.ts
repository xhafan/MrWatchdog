import Enumerable from "linq";
import { DomainConstants } from "../Shared/Generated/DomainConstants";
import { JobUrlConstants } from "../Shared/Generated/JobUrlConstants";
import { JobDto } from "../Shared/Generated/JobDto";
import { RebusConstants } from "../Shared/Generated/RebusConstants";
import { logError } from "../Shared/logging";

export function formSubmitWithWaitForJobCompletion(
    form: HTMLFormElement,
    onJobCompletion: (job: JobDto) => void,    
    confirmationMessage: string | undefined = undefined
) {
    form.onsubmit = async (event: SubmitEvent) => {
        event.preventDefault();

        if (!$(form).valid()) {
            return false;
        }

        if (confirmationMessage) {
            bootbox.confirm(
                confirmationMessage,
                result => {
                    if (!result) return;

                    sendRequestAndWaitForJobCompletionCommon();
                }
            );
        }
        else {
            sendRequestAndWaitForJobCompletionCommon();
        }
            
        function sendRequestAndWaitForJobCompletionCommon() {
            sendRequestAndWaitForJobCompletion(
                form.action, 
                new FormData(form), 
                event.submitter, 
                onJobCompletion, 
                form.method
            );
        }
    };
}

async function sendRequestAndWaitForJobCompletion(
    actionUrl: string,
    formData: FormData | undefined,
    submitter: HTMLElement | null = null,
    onJobCompletion: (job: JobDto) => void,
    method: string = 'POST'
) {       
    disableElementAndAddSpinner(submitter);

    try {

        const response = await fetch(actionUrl, {
            method,
            body: formData
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Failed to submit form: HTTP ${response.status} - ${errorText}`);
        }

        let jobGuid = await response.text()
        if (!jobGuid) {
            throw new Error("Error getting job Guid.");
        }

        const jobDto = await waitForJobCompletion(jobGuid);
       
        onJobCompletion(jobDto);
    }
    catch (error) {
        await logError(error, {}, true, true);
    }
       
    enableElementAndRemoveSpinner(submitter);
}

export async function waitForJobCompletion(jobGuid: string) : Promise<JobDto> {
    let getJobUrl = JobUrlConstants.getJobUrlTemplate.replace(JobUrlConstants.jobGuidVariable, jobGuid);

    const pollJob = async (delay = 300): Promise<JobDto> => {
        await new Promise(resolve => setTimeout(resolve, delay));
        const response = await fetch(getJobUrl);
        if (response.ok) {
            const jobDto = await response.json() as JobDto;
            if (jobDto.completedOn) {
                return jobDto;
            }
            else if (jobDto.numberOfHandlingAttempts >= RebusConstants.maxDeliveryAttempts) {
                throw new Error(`Job ${jobDto.guid} failed.`);
            }
        }
        return pollJob(Math.min(delay * 2, 8000));
    };

    const jobDto = await pollJob();
    
    return jobDto;
}

export async function getRelatedDomainEventJobGuid(commandJobGuid: string, domainEventType: string): Promise<string | null> {
    let getRelatedDomainEventJobUrl = JobUrlConstants.getRelatedDomainEventJobUrlTemplate
        .replace(JobUrlConstants.commandJobGuidVariable, commandJobGuid)
        .replace(JobUrlConstants.domainEventTypeVariable, domainEventType);

    const response = await fetch(getRelatedDomainEventJobUrl);
    if (response.ok) {
        const jobDto = await response.json() as JobDto;
        return jobDto.guid;
    }

    return null;
}

function disableElementAndAddSpinner(element: HTMLElement | null) {
    if (!element) return;

    element.setAttribute("disabled", "disabled");

    if (!element.querySelector("span[class^='spinner-border']")) {
        const spinner = document.createElement("span");
        spinner.className = "spinner-border spinner-border-sm me-2";
        element.prepend(spinner);
    }
}

function enableElementAndRemoveSpinner(element: HTMLElement | null) {
    if (!element) return;
    
    element.removeAttribute("disabled");
    element.querySelectorAll("span[class^='spinner-border']").forEach(spinnerSpan => spinnerSpan.remove());    
}
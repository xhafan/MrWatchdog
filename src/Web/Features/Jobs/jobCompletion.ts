import Enumerable from "linq";
import { DomainConstants } from "../Shared/Generated/DomainConstants";
import { JobUrlConstants } from "../Shared/Generated/JobUrlConstants";
import { JobDto } from "../Shared/Generated/JobDto";
import { RebusConstants } from "../Shared/Generated/RebusConstants";

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

export async function sendRequestAndWaitForJobCompletion(
    actionUrl: string,
    formData: FormData | undefined,
    submitter: HTMLElement | null = null,
    onJobCompletion: (job: JobDto) => void,
    method: string = 'POST'
) {       
    disableElementAndAddSpinner(submitter);

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
                let jobLastException = getJobLastException(jobDto);
                throw new Error(`Job ${jobDto.guid} failed: ${jobLastException}`);
            }
        }
        return pollJob(Math.min(delay * 2, 8000));
    };

    const jobDto = await pollJob();
    
    return jobDto;
}

function getJobLastException(jobDto: JobDto) : string {
    let lastJobHandlingAttempt = Enumerable.from(jobDto.handlingAttempts).orderByDescending(x => x.endedOn).first();
    return lastJobHandlingAttempt.exception;
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
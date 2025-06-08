import { DomainConstants } from "../Shared/Generated/DomainConstants";
import { JobConstants } from "../Shared/Generated/JobConstants";
import { JobDto } from "../Shared/Generated/JobDto";

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

    let getJobUrlWithReplacedJobGuid = JobConstants.getJobUrl.replace(JobConstants.jobGuidVariable, jobGuid);

    const pollJob = async (delay = 300): Promise<JobDto> => {
        await new Promise(resolve => setTimeout(resolve, delay));
        const response = await fetch(getJobUrlWithReplacedJobGuid);
        if (response.ok) {
            const jobDto = await response.json() as JobDto;
            if (jobDto.completedOn) {
                return jobDto;
            }
        }
        return pollJob(Math.min(delay * 2, 8000));
    };

    const jobDto = await pollJob();
        
    onJobCompletion(jobDto);
    enableElementAndRemoveSpinner(submitter);
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
import { DomainConstants } from "../Shared/Generated/DomainConstants";
import { JobDto } from "../Shared/Generated/JobDto";

export function formSubmitWithWaitForJobCompletion(
    form: HTMLFormElement,
    getJobUrl: string,
    onCompletion: (job: JobDto) => void
) {
    form.onsubmit = async (event: SubmitEvent) => {
        event.preventDefault();

        if (!$(form).valid()) {
            return false;
        }
        
        disableElementAndAddSpinner(event.submitter);

        const formData = new FormData(form);
        const action = form.action;
        const method = form.method;

        const response = await fetch(action, {
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

        getJobUrl = getJobUrl.replace("$jobGuid", jobGuid);

        const pollJob = async (delay = 300): Promise<JobDto> => {
            await new Promise(resolve => setTimeout(resolve, delay));
            const response = await fetch(getJobUrl);
            if (response.ok) {
                const jobDto = await response.json() as JobDto;
                if (jobDto.completedOn) {
                    return jobDto;
                }
            }
            return pollJob(Math.min(delay * 2, 8000));
        };

        const jobDto = await pollJob();
        
        onCompletion(jobDto);
    };
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
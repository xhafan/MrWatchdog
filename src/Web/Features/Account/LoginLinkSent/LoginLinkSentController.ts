import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import Enumerable from "linq";
import { AccountUrlConstants } from "../../Shared/Generated/AccountUrlConstants";
import BaseStimulusModelController from "../../Shared/BaseStimulusModelController";
import { LoginLinkSentStimulusModel } from "../../Shared/Generated/LoginLinkSentStimulusModel";
import { LoginTokenDto } from "../../Shared/Generated/LoginTokenDto";

export default class LoginLinkSentController extends BaseStimulusModelController<LoginLinkSentStimulusModel>  {
    connect() {

        const loginTokenGuid = this.modelValue.loginTokenGuid;
        const loginTokenConfirmationUrl = AccountUrlConstants.apiGetLoginTokenConfirmationUrl
            .replace(AccountUrlConstants.loginTokenGuidVariable, loginTokenGuid);

        const interval = setInterval(async () => {
            const loginTokenConfirmationResponse = await fetch(loginTokenConfirmationUrl);
            if (loginTokenConfirmationResponse.ok) {
                const loginTokenConfirmed = await loginTokenConfirmationResponse.json() as boolean;
                if (loginTokenConfirmed) {
                    clearInterval(interval);
                    const completeLoginResponse = await fetch(
                        AccountUrlConstants.apiCompleteLoginUrl.replace(AccountUrlConstants.loginTokenGuidVariable, loginTokenGuid), 
                        {
                            method: "POST"
                        }
                    );
                    if (completeLoginResponse.ok) {
                        Turbo.visit(completeLoginResponse.url);
                    } else {
                        throw new Error(`Error completing login: HTTP ${completeLoginResponse.status}`);
                    }
                }
            }
            else {
                clearInterval(interval);
                throw new Error(`Error getting login token confirmation: HTTP ${loginTokenConfirmationResponse.status}`);
            }
        }, 1000);
    }
}
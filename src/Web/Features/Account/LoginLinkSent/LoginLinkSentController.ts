import { Controller } from "@hotwired/stimulus";
import { formSubmitWithWaitForJobCompletion } from "../../Jobs/jobCompletion";
import Enumerable from "linq";
import { AccountUrlConstants } from "../../Shared/Generated/AccountUrlConstants";
import BaseStimulusModelController from "../../Shared/BaseStimulusModelController";
import { LoginLinkSentStimulusModel } from "../../Shared/Generated/LoginLinkSentStimulusModel";
import { LoginTokenDto } from "../../Shared/Generated/LoginTokenDto";

export default class LoginLinkSentController extends BaseStimulusModelController<LoginLinkSentStimulusModel>  {
    connected = false;

    connect() {
        this.connected = true;

        const loginTokenGuid = this.modelValue.loginTokenGuid;
        const loginTokenConfirmationUrl = AccountUrlConstants.apiGetLoginTokenConfirmationUrlTemplate
            .replace(AccountUrlConstants.loginTokenGuidVariable, loginTokenGuid);

        const interval = setInterval(async () => {
            const loginTokenConfirmationResponse = await fetch(loginTokenConfirmationUrl);
            if (loginTokenConfirmationResponse.ok) {
                const loginTokenConfirmed = await loginTokenConfirmationResponse.json() as boolean;
                if (loginTokenConfirmed) {
                    clearInterval(interval);
                    const completeLoginResponse = await fetch(
                        AccountUrlConstants.apiCompleteLoginUrlTemplate.replace(AccountUrlConstants.loginTokenGuidVariable, loginTokenGuid), 
                        {
                            method: "POST"
                        }
                    );
                    if (completeLoginResponse.ok) {
                        if (this.connected) {
                            const returnUrl = await completeLoginResponse.text();
                            Turbo.visit(returnUrl);
                        }
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

    disconnect() {
        this.connected = false;
    }
}
import type { TurboFrameLoadEvent } from "@hotwired/turbo";
import { Application } from "@hotwired/stimulus";
import { StimulusControllers } from "./Generated/StimulusControllers";
import { logError } from "./logging";

import "./site.css";


import BodyController from "./BodyController";
import TurboFrameController from "./TagHelpers/TurboFrame/TurboFrameController";
import ViewOrEditFormController from "./TagHelpers/ViewOrEditForm/ViewOrEditFormController";

import WatchdogsCreateController from "../Watchdogs/Create/CreateController";
import WatchdogsDetailController from "../Watchdogs/Detail/DetailController";
import WatchdogsDetailActionsController from "../Watchdogs/Detail/Actions/ActionsController";
import WatchdogsDetailWebPageController from "../Watchdogs/Detail/WebPage/WebPageController";
import WatchdogsDetailWebPageOverviewController from "../Watchdogs/Detail/WebPage/WebPageOverviewController";
import WatchdogsDetailWebPageDisabledWarningController from "../Watchdogs/Detail/WebPage/WebPageDisabledWarningController";
import WatchdogsDetailWebPageScrapingResultsController from "../Watchdogs/Detail/WebPage/WebPageScrapingResultsController";
import WatchdogsScrapingResultsController from "../Watchdogs/ScrapingResults/ScrapingResultsController";
import WatchdogsScrapingResultsWebPagesController from "../Watchdogs/Shared/ScrapingResultsWebPages/ScrapingResultsWebPagesController";
import WatchdogsAlertController from "../Watchdogs/Alert/AlertController";
import WatchdogsAlertOverviewController from "../Watchdogs/Alert/Overview/OverviewController";

import AccountLoginController from "../Account/Login/LoginController";
import AccountLoginLinkSentController from "../Account/LoginLinkSent/LoginLinkSentController";


const application = Application.start();
application.register(StimulusControllers.body, BodyController);
application.register(StimulusControllers.turboFrame, TurboFrameController);
application.register(StimulusControllers.viewOrEditForm, ViewOrEditFormController);

application.register(StimulusControllers.watchdogsCreate, WatchdogsCreateController);
application.register(StimulusControllers.watchdogsDetail, WatchdogsDetailController);
application.register(StimulusControllers.watchdogsDetailActions, WatchdogsDetailActionsController);
application.register(StimulusControllers.watchdogsDetailWebPage, WatchdogsDetailWebPageController);
application.register(StimulusControllers.watchdogsDetailWebPageOverview, WatchdogsDetailWebPageOverviewController);
application.register(StimulusControllers.watchdogsDetailWebPageDisabledWarning, WatchdogsDetailWebPageDisabledWarningController);
application.register(StimulusControllers.watchdogsDetailWebPageScrapingResults, WatchdogsDetailWebPageScrapingResultsController);
application.register(StimulusControllers.watchdogsScrapingResults, WatchdogsScrapingResultsController);
application.register(StimulusControllers.watchdogsScrapingResultsWebPages, WatchdogsScrapingResultsWebPagesController);
application.register(StimulusControllers.watchdogAlert, WatchdogsAlertController);
application.register(StimulusControllers.watchdogAlertOverview, WatchdogsAlertOverviewController);

application.register(StimulusControllers.accountLogin, AccountLoginController);
application.register(StimulusControllers.accountLoginLinkSent, AccountLoginLinkSentController);


attachValidationOnTurboLoad();
scrollToFragmentIdentifierOnTurboLoad();
handleErrorsGlobally();


function attachValidationOnTurboLoad() {
    document.addEventListener("turbo:load", () => {
        $.validator.unobtrusive.parse(document);
    });

    document.addEventListener("turbo:frame-load", (event: TurboFrameLoadEvent) => {
        const target = event.target as Element;
        $.validator.unobtrusive.parse(target);
    });
}

function scrollToFragmentIdentifierOnTurboLoad() {
    document.addEventListener("turbo:load", () => {
        scrollFragmentIdentifierElementIntoView();
    });

    document.addEventListener("turbo:frame-load", (event: TurboFrameLoadEvent) => {
        scrollFragmentIdentifierElementIntoView();
    });

    function scrollFragmentIdentifierElementIntoView() {
        if (window.location.hash) {
            const el = document.querySelector(window.location.hash);
            if (el) {
                el.scrollIntoView();
            }
        }
    }
}

function handleErrorsGlobally() {
    
    window.onerror = async (message, source, lineno, colno, error) => {
        await logError(error ?? message, {source, lineno, colno});
    };

    window.addEventListener("unhandledrejection", async (event: PromiseRejectionEvent) => {
        await logError(event.reason);
    });

    application.handleError = async (error: Error, message: string, detail: object) => {
        console.error(error, message, detail);

        // @ts-ignore
        let stimulusIdentifier = detail?.identifier;
        // @ts-ignore
        let elementId = detail?.element?.id;

        await logError(error, {stimulusIdentifier, elementId});
    }
}
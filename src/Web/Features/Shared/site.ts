import type { TurboFrameLoadEvent } from "@hotwired/turbo";
import { Application } from "@hotwired/stimulus";
import { StimulusControllers } from "./Generated/StimulusControllers";
import { logError } from "./logging";

import "./site.css";


import BodyController from "./BodyController";
import TurboFrameController from "./TagHelpers/TurboFrame/TurboFrameController";
import OnboardingController from "./TagHelpers/Onboarding/OnboardingController";
import ViewOrEditFormController from "./TagHelpers/ViewOrEditForm/ViewOrEditFormController";

import ScrapersCreateController from "../Scrapers/Create/CreateController";
import ScrapersDetailController from "../Scrapers/Detail/DetailController";
import ScrapersDetailActionsController from "../Scrapers/Detail/Actions/ActionsController";
import ScrapersDetailWebPageController from "../Scrapers/Detail/WebPage/WebPageController";
import ScrapersDetailWebPageOverviewController from "../Scrapers/Detail/WebPage/WebPageOverviewController";
import ScrapersDetailWebPageDisabledWarningController from "../Scrapers/Detail/WebPage/WebPageDisabledWarningController";
import ScrapersDetailWebPageScrapingResultsController from "../Scrapers/Detail/WebPage/WebPageScrapingResultsController";
import ScrapersScrapingResultsController from "../Scrapers/ScrapingResults/ScrapingResultsController";
import ScrapersScrapingResultsWebPagesController from "../Scrapers/Shared/ScrapingResultsWebPages/ScrapingResultsWebPagesController";
import ScrapersSearchController from "../Scrapers/Search/SearchController";
import ScrapersSearchOverviewController from "../Scrapers/Search/Overview/OverviewController";

import AccountLoginController from "../Account/Login/LoginController";
import AccountLoginLinkSentController from "../Account/LoginLinkSent/LoginLinkSentController";


const application = Application.start();
application.register(StimulusControllers.body, BodyController);
application.register(StimulusControllers.turboFrame, TurboFrameController);
application.register(StimulusControllers.onboarding, OnboardingController);
application.register(StimulusControllers.viewOrEditForm, ViewOrEditFormController);

application.register(StimulusControllers.scrapersCreate, ScrapersCreateController);
application.register(StimulusControllers.scrapersDetail, ScrapersDetailController);
application.register(StimulusControllers.scrapersDetailActions, ScrapersDetailActionsController);
application.register(StimulusControllers.scrapersDetailWebPage, ScrapersDetailWebPageController);
application.register(StimulusControllers.scrapersDetailWebPageOverview, ScrapersDetailWebPageOverviewController);
application.register(StimulusControllers.scrapersDetailWebPageDisabledWarning, ScrapersDetailWebPageDisabledWarningController);
application.register(StimulusControllers.scrapersDetailWebPageScrapingResults, ScrapersDetailWebPageScrapingResultsController);
application.register(StimulusControllers.scrapersScrapingResults, ScrapersScrapingResultsController);
application.register(StimulusControllers.scrapersScrapingResultsWebPages, ScrapersScrapingResultsWebPagesController);
application.register(StimulusControllers.watchdogSearch, ScrapersSearchController);
application.register(StimulusControllers.watchdogSearchOverview, ScrapersSearchOverviewController);

application.register(StimulusControllers.accountLogin, AccountLoginController);
application.register(StimulusControllers.accountLoginLinkSent, AccountLoginLinkSentController);


attachValidationOnTurboLoad();
scrollToFragmentIdentifierOnTurboLoad();
handleErrorsGlobally();

console.log("App reloaded");


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
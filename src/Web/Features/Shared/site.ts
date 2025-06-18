import type { TurboFrameLoadEvent } from "@hotwired/turbo";
import { Application } from "@hotwired/stimulus";
import { StimulusControllers } from "./Generated/StimulusControllers";

import "./site.css";


import BodyController from "./BodyController";
import TurboFrameController from "./TagHelpers/TurboFrame/TurboFrameController";
import ViewOrEditFormController from "./TagHelpers/ViewOrEditForm/ViewOrEditFormController";

import WatchdogsCreateController from "../Watchdogs/Create/CreateController";
import WatchdogsDetailController from "../Watchdogs/Detail/DetailController";
import WatchdogsDetailWebPageController from "../Watchdogs/Detail/WebPage/WebPageController";
import WatchdogsDetailWebPageOverviewController from "../Watchdogs/Detail/WebPage/WebPageOverviewController";


const application = Application.start();
application.register(StimulusControllers.body, BodyController);
application.register(StimulusControllers.turboFrame, TurboFrameController);
application.register(StimulusControllers.viewOrEditForm, ViewOrEditFormController);

application.register(StimulusControllers.watchdogsCreate, WatchdogsCreateController);
application.register(StimulusControllers.watchdogsDetail, WatchdogsDetailController);
application.register(StimulusControllers.watchdogsDetailWebPage, WatchdogsDetailWebPageController);
application.register(StimulusControllers.watchdogsDetailWebPageOverview, WatchdogsDetailWebPageOverviewController);


attachValidationAfterTurboLoad();

function attachValidationAfterTurboLoad() {
    document.addEventListener("turbo:load", function () {
        $.validator.unobtrusive.parse(document);
    });

    document.addEventListener("turbo:frame-load", function (e: TurboFrameLoadEvent) {
        const target = e.target as Element;
        $.validator.unobtrusive.parse(target);
    });
}

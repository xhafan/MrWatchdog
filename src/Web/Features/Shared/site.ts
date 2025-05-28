import type { TurboFrameLoadEvent } from "@hotwired/turbo";
import { Application } from "@hotwired/stimulus";
import { StimulusControllers } from "./Generated/StimulusControllers";

import "./site.css";

import WatchdogsCreateController from "../Watchdogs/create";
import WatchdogsDetailController from "../Watchdogs/detail";


const application = Application.start();
application.register(StimulusControllers.watchdogsCreate, WatchdogsCreateController);
application.register(StimulusControllers.watchdogsDetail, WatchdogsDetailController);

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

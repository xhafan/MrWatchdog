import type { TurboFrameLoadEvent } from "@hotwired/turbo";
import { Application } from "@hotwired/stimulus";
import WatchdogsDetailController from "../Watchdogs/Detail";

import "./site.css";

const application = Application.start();
application.register("watchdogs--detail", WatchdogsDetailController);

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

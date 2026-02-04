import { Controller } from "@hotwired/stimulus";
import { searchTermModifiedEventName } from "../../../Scrapers/Shared/ScrapedResultsWebPages/ScrapedResultsWebPagesController";

export default class OverviewController extends Controller {
    static targets  = [
        "searchTerm"
    ];

    declare searchTermTarget: HTMLInputElement;

    connect() {
        this.onSearchTermModified();
    }

    onSearchTermModified() {
        let searchTerm = this.searchTermTarget.value;

        this.dispatch(searchTermModifiedEventName, { detail: searchTerm, prefix: "" });
    }
}
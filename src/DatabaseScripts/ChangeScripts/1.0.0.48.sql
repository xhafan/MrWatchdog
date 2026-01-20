alter table "ScraperWebPage" add "ScrapeHtmlAsRenderedByBrowser" bool not null default(false);
alter table "ScraperWebPage" alter "ScrapeHtmlAsRenderedByBrowser" drop default;
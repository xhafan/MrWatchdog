alter table "Scraper" add "ScrapedResultsFilteringNotSupported" boolean not null default(false);
alter table "Scraper" alter column "ScrapedResultsFilteringNotSupported" drop default;

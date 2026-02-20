ALTER TABLE "WatchdogScrapedResultHistory" RENAME COLUMN "Result" TO "ScrapedResultValue";
ALTER TABLE "WatchdogScrapedResultHistory" add "ScrapedResultHash" bytea not null default(''::bytea);
ALTER TABLE "WatchdogScrapedResultHistory" alter column "ScrapedResultHash" drop default;

ALTER TABLE "WatchdogCurrentScrapedResult" RENAME COLUMN "Value" TO "ScrapedResultValue";
ALTER TABLE "WatchdogCurrentScrapedResult" add "ScrapedResultHash" bytea not null default(''::bytea);
ALTER TABLE "WatchdogCurrentScrapedResult" alter column "ScrapedResultHash" drop default;

ALTER TABLE "WatchdogScrapedResultToNotifyAbout" RENAME COLUMN "Value" TO "ScrapedResultValue";
ALTER TABLE "WatchdogScrapedResultToNotifyAbout" add "ScrapedResultHash" bytea not null default(''::bytea);
ALTER TABLE "WatchdogScrapedResultToNotifyAbout" alter column "ScrapedResultHash" drop default;

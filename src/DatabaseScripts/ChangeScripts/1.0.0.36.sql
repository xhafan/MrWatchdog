ALTER TABLE "Watchdog" add "NumberOfFailedScrapingAttemptsBeforeEmailAlert" int4 not null default(5);
ALTER TABLE "Watchdog" alter column "NumberOfFailedScrapingAttemptsBeforeEmailAlert" drop default;

ALTER TABLE "WatchdogWebPage" add "NumberOfFailedScrapingAttempts" int4 not null default(0);
ALTER TABLE "WatchdogWebPage" alter column "NumberOfFailedScrapingAttempts" drop default;

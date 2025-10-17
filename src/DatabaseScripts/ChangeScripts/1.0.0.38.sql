ALTER TABLE "Watchdog" RENAME COLUMN "NumberOfFailedScrapingAttemptsBeforeEmailAlert" TO "NumberOfFailedScrapingAttemptsBeforeAlerting";
ALTER TABLE "WatchdogWebPage" RENAME COLUMN "NumberOfFailedScrapingAttempts" TO "NumberOfFailedScrapingAttemptsBeforeTheNextAlert";

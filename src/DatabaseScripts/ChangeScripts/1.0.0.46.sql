ALTER TABLE "WatchdogSearch" RENAME TO "Watchdog";
ALTER TABLE "Watchdog" RENAME CONSTRAINT "FK_WatchdogSearch_Scraper" TO "FK_Watchdog_Scraper";
ALTER TABLE "Watchdog" RENAME CONSTRAINT "FK_WatchdogSearch_User" TO "FK_Watchdog_User";
ALTER INDEX "WatchdogSearch_User_idx" RENAME TO "Watchdog_User_idx";
ALTER INDEX "WatchdogSearch_WatchdogId_idx" RENAME TO "Watchdog_ScraperId_idx";
ALTER INDEX "WatchdogSearch_pkey" RENAME TO "Watchdog_pkey";

ALTER TABLE "WatchdogSearchCurrentScrapingResult" RENAME TO "WatchdogCurrentScrapingResult";
ALTER TABLE "WatchdogCurrentScrapingResult" RENAME COLUMN "WatchdogSearchId" TO "WatchdogId";
ALTER TABLE "WatchdogCurrentScrapingResult" RENAME CONSTRAINT "FK_WatchdogSearchCurrentScrapingResult_WatchdogSearch" TO "FK_WatchdogCurrentScrapingResult_Watchdog";
ALTER INDEX "WatchdogSearchCurrentScrapingResult_WatchdogSearchId_idx" RENAME TO "WatchdogCurrentScrapingResult_WatchdogId_idx";
ALTER INDEX "WatchdogSearchCurrentScrapingResult_pkey" RENAME TO "WatchdogCurrentScrapingResult_pkey";

ALTER TABLE "WatchdogSearchScrapingResultHistory" RENAME TO "WatchdogScrapingResultHistory";
ALTER TABLE "WatchdogScrapingResultHistory" RENAME COLUMN "WatchdogSearchId" TO "WatchdogId";
ALTER TABLE "WatchdogScrapingResultHistory" RENAME CONSTRAINT "FK_WatchdogSearchScrapingResultHistory_WatchdogSearch" TO "FK_WatchdogScrapingResultHistory_Watchdog";
ALTER INDEX "WatchdogSearchScrapingResultHistory_WatchdogSearchId_idx" RENAME TO "WatchdogScrapingResultHistory_WatchdogId_idx";
ALTER INDEX "WatchdogSearchScrapingResultHistory_pkey" RENAME TO "WatchdogScrapingResultHistory_pkey";

ALTER TABLE "WatchdogSearchScrapingResultToNotifyAbout" RENAME TO "WatchdogScrapingResultToNotifyAbout";
ALTER TABLE "WatchdogScrapingResultToNotifyAbout" RENAME COLUMN "WatchdogSearchId" TO "WatchdogId";
ALTER TABLE "WatchdogScrapingResultToNotifyAbout" RENAME CONSTRAINT "FK_WatchdogSearchScrapingResultToNotifyAbout_WatchdogSearch" TO "FK_WatchdogScrapingResultToNotifyAbout_Watchdog";
ALTER INDEX "WatchdogSearchScrapingResultToNotifyAbout_WatchdogSearchId_idx" RENAME TO "WatchdogScrapingResultToNotifyAbout_WatchdogId_idx";
ALTER INDEX "WatchdogSearchScrapingResultToNotifyAbout_pkey" RENAME TO "WatchdogScrapingResultToNotifyAbout_pkey";

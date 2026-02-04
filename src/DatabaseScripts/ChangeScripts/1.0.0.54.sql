ALTER TABLE "ScraperWebPageScrapingResult" RENAME TO "ScraperWebPageScrapedResult";
ALTER TABLE "ScraperWebPageScrapedResult" RENAME CONSTRAINT "FK_ScraperWebPageScrapingResult_ScraperWebPage" TO "FK_ScraperWebPageScrapedResult_ScraperWebPage";
ALTER INDEX "ScraperWebPageScrapingResult_ScraperWebPageId_idx" RENAME TO "ScraperWebPageScrapedResult_ScraperWebPageId_idx";
ALTER INDEX "ScraperWebPageScrapingResult_pkey" RENAME TO "ScraperWebPageScrapedResult_pkey";

ALTER TABLE "WatchdogCurrentScrapingResult" RENAME TO "WatchdogCurrentScrapedResult";
ALTER TABLE "WatchdogCurrentScrapedResult" RENAME CONSTRAINT "FK_WatchdogCurrentScrapingResult_Watchdog" TO "FK_WatchdogCurrentScrapedResult_Watchdog";
ALTER INDEX "WatchdogCurrentScrapingResult_WatchdogId_idx" RENAME TO "WatchdogCurrentScrapedResult_WatchdogId_idx";
ALTER INDEX "WatchdogCurrentScrapingResult_pkey" RENAME TO "WatchdogCurrentScrapedResult_pkey";

ALTER TABLE "WatchdogScrapingResultHistory" RENAME TO "WatchdogScrapedResultHistory";
ALTER TABLE "WatchdogScrapedResultHistory" RENAME CONSTRAINT "FK_WatchdogScrapingResultHistory_Watchdog" TO "FK_WatchdogScrapedResultHistory_Watchdog";
ALTER INDEX "WatchdogScrapingResultHistory_WatchdogId_idx" RENAME TO "WatchdogScrapedResultHistory_WatchdogId_idx";
ALTER INDEX "WatchdogScrapingResultHistory_pkey" RENAME TO "WatchdogScrapedResultHistory_pkey";

ALTER TABLE "WatchdogScrapingResultToNotifyAbout" RENAME TO "WatchdogScrapedResultToNotifyAbout";
ALTER TABLE "WatchdogScrapedResultToNotifyAbout" RENAME CONSTRAINT "FK_WatchdogScrapingResultToNotifyAbout_Watchdog" TO "FK_WatchdogScrapedResultToNotifyAbout_Watchdog";
ALTER INDEX "WatchdogScrapingResultToNotifyAbout_WatchdogId_idx" RENAME TO "WatchdogScrapedResultToNotifyAbout_WatchdogId_idx";
ALTER INDEX "WatchdogScrapingResultToNotifyAbout_pkey" RENAME TO "WatchdogScrapedResultToNotifyAbout_pkey";
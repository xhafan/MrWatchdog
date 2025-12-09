ALTER TABLE "Watchdog" RENAME TO "Scraper";
ALTER TABLE "Scraper" RENAME CONSTRAINT "FK_Watchdog_User" TO "FK_Scraper_User";
ALTER INDEX "Watchdog_PublicStatus_MakePublicRequested_idx" RENAME TO "Scraper_PublicStatus_RequestedToBeMadePublic_idx";
ALTER INDEX "Watchdog_PublicStatus_Public_idx" RENAME TO "Scraper_PublicStatus_Public_idx";
ALTER INDEX "Watchdog_User_idx" RENAME TO "Scraper_User_idx";
ALTER INDEX "Watchdog_pkey" RENAME TO "Scraper_pkey";

ALTER TABLE "WatchdogWebPage" RENAME TO "ScraperWebPage";
ALTER TABLE "ScraperWebPage" RENAME COLUMN "WatchdogId" TO "ScraperId";
ALTER TABLE "ScraperWebPage" RENAME CONSTRAINT "FK_WatchdogWebPage_Watchdog" TO "FK_ScraperWebPage_Scraper";
ALTER INDEX "WatchdogWebPage_WatchdogId_idx" RENAME TO "ScraperWebPage_ScraperId_idx";
ALTER INDEX "WatchdogWebPage_pkey" RENAME TO "ScraperWebPage_pkey";

ALTER TABLE "WatchdogWebPageHttpHeaders" RENAME TO "ScraperWebPageHttpHeader";
ALTER TABLE "ScraperWebPageHttpHeader" RENAME COLUMN "WatchdogWebPageId" TO "ScraperWebPageId";
ALTER TABLE "ScraperWebPageHttpHeader" RENAME CONSTRAINT "FK_WatchdogWebPageHttpHeaders_WatchdogWebPage" TO "FK_ScraperWebPageHttpHeader_ScraperWebPage";
ALTER INDEX "WatchdogWebPageHttpHeaders_WatchdogWebPageId_idx" RENAME TO "ScraperWebPageHttpHeader_ScraperWebPageId_idx";
ALTER INDEX "WatchdogWebPageHttpHeaders_pkey" RENAME TO "ScraperWebPageHttpHeader_pkey";

ALTER TABLE "WatchdogWebPageScrapingResult" RENAME TO "ScraperWebPageScrapingResult";
ALTER TABLE "ScraperWebPageScrapingResult" RENAME COLUMN "WatchdogWebPageId" TO "ScraperWebPageId";
ALTER TABLE "ScraperWebPageScrapingResult" RENAME CONSTRAINT "FK_WatchdogWebPageScrapingResult_WatchdogWebPage" TO "FK_ScraperWebPageScrapingResult_ScraperWebPage";
ALTER INDEX "WatchdogWebPageScrapingResult_WatchdogWebPageId_idx" RENAME TO "ScraperWebPageScrapingResult_ScraperWebPageId_idx";
ALTER INDEX "WatchdogWebPageScrapingResult_pkey" RENAME TO "ScraperWebPageScrapingResult_pkey";

ALTER TABLE "WatchdogSearch" RENAME COLUMN "WatchdogId" TO "ScraperId";
ALTER TABLE "WatchdogSearch" RENAME CONSTRAINT "FK_WatchdogSearch_Watchdog" TO "FK_WatchdogSearch_Scraper";

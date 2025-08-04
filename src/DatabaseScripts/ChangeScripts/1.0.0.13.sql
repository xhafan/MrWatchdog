ALTER TABLE "WatchdogWebPageSelectedElement" RENAME TO "WatchdogWebPageScrapingResult"; 
ALTER INDEX "WatchdogWebPageSelectedElement_pkey" RENAME TO "WatchdogWebPageScrapingResult_pkey";
ALTER INDEX "WatchdogWebPageSelectedElement_WatchdogWebPageId_idx" RENAME TO "WatchdogWebPageScrapingResult_WatchdogWebPageId_idx";
ALTER TABLE "WatchdogWebPageScrapingResult" RENAME CONSTRAINT "FK_WatchdogWebPageSelectedElement_WatchdogWebPage" TO "FK_WatchdogWebPageScrapingResult_WatchdogWebPage";

ALTER TABLE "WatchdogAlertPreviousScrapedResult" RENAME TO "WatchdogAlertPreviousScrapingResult"; 
ALTER INDEX "WatchdogAlertPreviousScrapedResult_pkey" RENAME TO "WatchdogAlertPreviousScrapingResult_pkey";
ALTER INDEX "WatchdogAlertPreviousScrapedResult_WatchdogAlertId_idx" RENAME TO "WatchdogAlertPreviousScrapingResult_WatchdogAlertId_idx";
ALTER TABLE "WatchdogAlertPreviousScrapingResult" RENAME CONSTRAINT "FK_WatchdogAlertPreviousScrapedResult_WatchdogAlert" TO "FK_WatchdogAlertPreviousScrapingResult_WatchdogAlert";

ALTER TABLE "WatchdogAlertCurrentScrapedResult" RENAME TO "WatchdogAlertCurrentScrapingResult"; 
ALTER INDEX "WatchdogAlertCurrentScrapedResult_pkey" RENAME TO "WatchdogAlertCurrentScrapingResult_pkey";
ALTER INDEX "WatchdogAlertCurrentScrapedResult_WatchdogAlertId_idx" RENAME TO "WatchdogAlertCurrentScrapingResult_WatchdogAlertId_idx";
ALTER TABLE "WatchdogAlertCurrentScrapingResult" RENAME CONSTRAINT "FK_WatchdogAlertCurrentScrapedResult_WatchdogAlert" TO "FK_WatchdogAlertCurrentScrapingResult_WatchdogAlert";
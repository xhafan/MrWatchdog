ALTER TABLE "WatchdogAlert" RENAME TO "WatchdogSearch";
ALTER TABLE "WatchdogSearch" RENAME CONSTRAINT "FK_WatchdogAlert_User" TO "FK_WatchdogSearch_User";
ALTER TABLE "WatchdogSearch" RENAME CONSTRAINT "FK_WatchdogAlert_Watchdog" TO "FK_WatchdogSearch_Watchdog";
ALTER INDEX "WatchdogAlert_User_idx" RENAME TO "WatchdogSearch_User_idx";
ALTER INDEX "WatchdogAlert_WatchdogId_idx" RENAME TO "WatchdogSearch_WatchdogId_idx";
ALTER INDEX "WatchdogAlert_pkey" RENAME TO "WatchdogSearch_pkey";

ALTER TABLE "WatchdogAlertCurrentScrapingResult" RENAME TO "WatchdogSearchCurrentScrapingResult";
ALTER TABLE "WatchdogSearchCurrentScrapingResult" RENAME COLUMN "WatchdogAlertId" TO "WatchdogSearchId";
ALTER TABLE "WatchdogSearchCurrentScrapingResult" RENAME CONSTRAINT "FK_WatchdogAlertCurrentScrapingResult_WatchdogAlert" TO "FK_WatchdogSearchCurrentScrapingResult_WatchdogSearch";
ALTER INDEX "WatchdogAlertCurrentScrapingResult_WatchdogAlertId_idx" RENAME TO "WatchdogSearchCurrentScrapingResult_WatchdogSearchId_idx";
ALTER INDEX "WatchdogAlertCurrentScrapingResult_pkey" RENAME TO "WatchdogSearchCurrentScrapingResult_pkey";

ALTER TABLE "WatchdogAlertScrapingResultAlertHistory" RENAME TO "WatchdogSearchScrapingResultHistory";
ALTER TABLE "WatchdogSearchScrapingResultHistory" RENAME COLUMN "WatchdogAlertId" TO "WatchdogSearchId";
ALTER TABLE "WatchdogSearchScrapingResultHistory" RENAME COLUMN "AlertedOn" TO "NotifiedOn"; 
ALTER TABLE "WatchdogSearchScrapingResultHistory" RENAME CONSTRAINT "FK_WatchdogAlertScrapingResultAlertHistory_WatchdogAlert" TO "FK_WatchdogSearchScrapingResultHistory_WatchdogSearch";
ALTER INDEX "WatchdogAlertScrapingResultAlertHistory_WatchdogAlertId_idx" RENAME TO "WatchdogSearchScrapingResultHistory_WatchdogSearchId_idx";
ALTER INDEX "WatchdogAlertScrapingResultAlertHistory_pkey" RENAME TO "WatchdogSearchScrapingResultHistory_pkey";

ALTER TABLE "WatchdogAlertScrapingResultToAlertAbout" RENAME TO "WatchdogSearchScrapingResultToNotifyAbout";
ALTER TABLE "WatchdogSearchScrapingResultToNotifyAbout" RENAME COLUMN "WatchdogAlertId" TO "WatchdogSearchId";
ALTER TABLE "WatchdogSearchScrapingResultToNotifyAbout" RENAME CONSTRAINT "FK_WatchdogAlertScrapingResultToAlertAbout_WatchdogAlert" TO "FK_WatchdogSearchScrapingResultToNotifyAbout_WatchdogSearch";
ALTER INDEX "WatchdogAlertScrapingResultToAlertAbout_WatchdogAlertId_idx" RENAME TO "WatchdogSearchScrapingResultToNotifyAbout_WatchdogSearchId_idx";
ALTER INDEX "WatchdogAlertScrapingResultToAlertAbout_pkey" RENAME TO "WatchdogSearchScrapingResultToNotifyAbout_pkey";

ALTER TABLE "Watchdog" RENAME COLUMN "IntervalBetweenSameResultAlertsInDays" TO "IntervalBetweenSameResultNotificationsInDays";
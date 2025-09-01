DROP INDEX "WatchdogAlert_WatchdogId_SearchTerm_key";

CREATE UNIQUE INDEX "WatchdogAlert_WatchdogId_UserId_SearchTerm_key" ON "WatchdogAlert" ("WatchdogId", "UserId", "SearchTerm");
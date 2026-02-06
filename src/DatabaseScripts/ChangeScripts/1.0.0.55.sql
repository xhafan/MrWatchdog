ALTER TABLE "WatchdogScrapedResultHistory" RENAME TO "WatchdogScrapedResultHistoryOld";
ALTER TABLE "WatchdogScrapedResultHistoryOld" RENAME CONSTRAINT "FK_WatchdogScrapedResultHistory_Watchdog" TO "FK_WatchdogScrapedResultHistoryOld_Watchdog";
ALTER INDEX "WatchdogScrapedResultHistory_WatchdogId_idx" RENAME TO "WatchdogScrapedResultHistoryOld_WatchdogId_idx";
ALTER INDEX "WatchdogScrapedResultHistory_pkey" RENAME TO "WatchdogScrapedResultHistoryOld_pkey";

create table "WatchdogScrapedResultHistory" (
    "Id" int8 not null,
   "Version" int8 not null,
   "Result" text not null,
   "NotifiedOn" timestamp not null,
   "WatchdogId" int8,
   primary key ("Id")
);

alter table "WatchdogScrapedResultHistory" 
    add constraint "FK_WatchdogScrapedResultHistory_Watchdog"
    foreign key ("WatchdogId") 
    references "Watchdog";

CREATE INDEX "WatchdogScrapedResultHistory_WatchdogId_idx" ON "WatchdogScrapedResultHistory" ("WatchdogId");

INSERT INTO "WatchdogScrapedResultHistory" (
    "Id", 
    "Version", 
    "Result", 
    "NotifiedOn", 
    "WatchdogId"
)
SELECT 
    row_number() OVER (ORDER BY "NotifiedOn" ASC) AS "Id",
    1 AS "Version",
    "Result",
    "NotifiedOn",
    "WatchdogId"
from "WatchdogScrapedResultHistoryOld";

drop table "WatchdogScrapedResultHistoryOld";
create table "WatchdogAlertScrapingResultAlertHistory" (
    "WatchdogAlertId" int8 not null,
   "Result" text not null,
   "AlertedOn" timestamp not null,
   primary key ("WatchdogAlertId", "Result", "AlertedOn")
);

alter table "WatchdogAlertScrapingResultAlertHistory" 
    add constraint "FK_WatchdogAlertScrapingResultAlertHistory_WatchdogAlert"
    foreign key ("WatchdogAlertId") 
    references "WatchdogAlert";

create index "WatchdogAlertScrapingResultAlertHistory_WatchdogAlertId_idx" on "WatchdogAlertScrapingResultAlertHistory" ("WatchdogAlertId");
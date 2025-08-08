create table "WatchdogAlertScrapingResultToAlertAbout" (
    "WatchdogAlertId" int8 not null,
    "Value" text not null,
    "Index" int4 not null,
    primary key ("WatchdogAlertId", "Index")
);

alter table "WatchdogAlertScrapingResultToAlertAbout" 
    add constraint "FK_WatchdogAlertScrapingResultToAlertAbout_WatchdogAlert"
    foreign key ("WatchdogAlertId") 
    references "WatchdogAlert";

create index "WatchdogAlertScrapingResultToAlertAbout_WatchdogAlertId_idx" on "WatchdogAlertScrapingResultToAlertAbout" ("WatchdogAlertId");

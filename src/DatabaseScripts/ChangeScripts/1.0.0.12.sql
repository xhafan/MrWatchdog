drop table "WatchdogAlert";

create table "WatchdogAlert" (
    "Id" int8 not null,
   "Version" int8 not null,
   "WatchdogId" int8 not null,
   "SearchTerm" text,
   primary key ("Id")
);

create table "WatchdogAlertPreviousScrapedResult" (
    "WatchdogAlertId" int8 not null,
   "Value" text not null,
   "Index" int4 not null,
   primary key ("WatchdogAlertId", "Index")
);

create table "WatchdogAlertCurrentScrapedResult" (
    "WatchdogAlertId" int8 not null,
   "Value" text not null,
   "Index" int4 not null,
   primary key ("WatchdogAlertId", "Index")
);

alter table "WatchdogAlert" 
    add constraint "FK_WatchdogAlert_Watchdog"
    foreign key ("WatchdogId") 
    references "Watchdog";

alter table "WatchdogAlertPreviousScrapedResult" 
    add constraint "FK_WatchdogAlertPreviousScrapedResult_WatchdogAlert"
    foreign key ("WatchdogAlertId") 
    references "WatchdogAlert";

alter table "WatchdogAlertCurrentScrapedResult" 
    add constraint "FK_WatchdogAlertCurrentScrapedResult_WatchdogAlert"
    foreign key ("WatchdogAlertId") 
    references "WatchdogAlert";

create index "WatchdogAlert_WatchdogId_idx" on "WatchdogAlert" ("WatchdogId");
create unique index "WatchdogAlert_WatchdogId_SearchTerm_key" on "WatchdogAlert" ("WatchdogId", "SearchTerm");

create index "WatchdogAlertPreviousScrapedResult_WatchdogAlertId_idx" on "WatchdogAlertPreviousScrapedResult" ("WatchdogAlertId");
create index "WatchdogAlertCurrentScrapedResult_WatchdogAlertId_idx" on "WatchdogAlertCurrentScrapedResult" ("WatchdogAlertId");


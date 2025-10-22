create table "WatchdogWebPageHttpHeaders" (
    "WatchdogWebPageId" int8 not null,
   "Name" text not null,
   "Value" text not null,
   primary key ("WatchdogWebPageId", "Name", "Value")
);

alter table "WatchdogWebPageHttpHeaders" 
    add constraint "FK_WatchdogWebPageHttpHeaders_WatchdogWebPage"
    foreign key ("WatchdogWebPageId") 
    references "WatchdogWebPage";

create index "WatchdogWebPageHttpHeaders_WatchdogWebPageId_idx" on "WatchdogWebPageHttpHeaders" ("WatchdogWebPageId");

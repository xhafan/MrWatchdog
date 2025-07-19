alter table "WatchdogWebPage" add "SelectText" boolean not null default(false);
alter table "WatchdogWebPage" alter column "SelectText" drop default;

alter table "WatchdogWebPage" drop column "SelectedHtml";

create table "WatchdogWebPageSelectedElement" (
    "WatchdogWebPageId" int8 not null,
   "Value" text not null,
   "Index" int4 not null,
   primary key ("WatchdogWebPageId", "Index")
);

alter table "WatchdogWebPageSelectedElement" 
    add constraint "FK_WatchdogWebPageSelectedElement_WatchdogWebPage" 
    foreign key ("WatchdogWebPageId") 
    references "WatchdogWebPage";

create index "WatchdogWebPageSelectedElement_WatchdogWebPageId_idx" on "WatchdogWebPageSelectedElement" ("WatchdogWebPageId");
alter table "Watchdog"
    add "ScrapingIntervalInSeconds" int4 not null default(86400),
    add "NextScrapingOn" timestamp null;

alter table "Watchdog" alter column "ScrapingIntervalInSeconds" drop default;
alter table "WatchdogWebPage" add "IsEnabled" bool not null default(true);
alter table "WatchdogWebPage" alter column "IsEnabled" drop default;


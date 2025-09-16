alter table "Watchdog" add "CanNotifyAboutFailedScraping" bool not null default(false);
alter table "Watchdog" alter column "CanNotifyAboutFailedScraping" drop default;

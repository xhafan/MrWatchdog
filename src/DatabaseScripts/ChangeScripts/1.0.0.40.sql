ALTER TABLE "WatchdogSearch" add "IsArchived" bool not null default(false);
ALTER TABLE "WatchdogSearch" alter column "IsArchived" drop default;

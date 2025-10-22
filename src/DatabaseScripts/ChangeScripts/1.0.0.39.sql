ALTER TABLE "Watchdog" add "IsArchived" bool not null default(false);
ALTER TABLE "Watchdog" alter column "IsArchived" drop default;


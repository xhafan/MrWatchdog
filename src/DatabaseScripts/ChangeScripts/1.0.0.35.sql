ALTER TABLE "WatchdogSearch" add "ReceiveNotification" bool not null default(true);
ALTER TABLE "WatchdogSearch" alter column "ReceiveNotification" drop default;

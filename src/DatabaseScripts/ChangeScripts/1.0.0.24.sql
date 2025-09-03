alter table "Watchdog" add "MakePublicRequested" boolean not null default(false);
alter table "Watchdog" alter column "MakePublicRequested" drop default;

alter table "Watchdog" add "Public" boolean not null default(false);
alter table "Watchdog" alter column "Public" drop default;

CREATE INDEX "Watchdog_MakePublicRequested_true_idx" ON "Watchdog" ("MakePublicRequested") WHERE "MakePublicRequested" = true;
CREATE INDEX "Watchdog_Public_true_idx" ON "Watchdog" ("Public") WHERE "Public" = true;
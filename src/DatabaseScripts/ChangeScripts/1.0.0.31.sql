alter table "Watchdog" add "PublicStatus" char(1) not null default('T');
alter table "Watchdog" alter column "PublicStatus" drop default;

CREATE INDEX "Watchdog_PublicStatus_Public_idx" ON "Watchdog" ("PublicStatus") WHERE ("PublicStatus" = 'P');
CREATE INDEX "Watchdog_PublicStatus_MakePublicRequested_idx" ON "Watchdog" ("PublicStatus") WHERE ("PublicStatus" = 'R');

update "Watchdog" set
    "PublicStatus" = 
        case
            when "Public" = true then 'P'
            when "MakePublicRequested" = true then 'R'
            else 'T'
        end
;

alter table "Watchdog" drop column "Public";
alter table "Watchdog" drop column "MakePublicRequested";
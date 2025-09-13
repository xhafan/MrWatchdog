alter table "Watchdog" add "IntervalBetweenSameResultAlertsInDays" float8 not null default(30);
alter table "Watchdog" alter column "IntervalBetweenSameResultAlertsInDays" drop default;
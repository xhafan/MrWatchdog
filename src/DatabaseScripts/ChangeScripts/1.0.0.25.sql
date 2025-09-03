alter table "User" add "SuperAdmin" boolean not null default(false);
alter table "User" alter column "SuperAdmin" drop default;

alter table "User" add "Culture" text not null default('en');
alter table "User" alter column "Culture" drop default;
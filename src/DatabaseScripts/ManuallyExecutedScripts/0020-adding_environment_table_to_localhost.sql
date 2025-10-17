create table if not exists "Environment" ("Value" text not null); 
CREATE unique INDEX if not exists "Environment_Value_idx" on "Environment"(("Value" is not null));
insert into "Environment" values ('Development') on conflict do nothing;

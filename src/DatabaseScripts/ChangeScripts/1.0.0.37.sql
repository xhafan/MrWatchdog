create table if not exists "Environment" ("Value" text not null); 
CREATE unique INDEX if not exists "Environment_Value_idx" on "Environment"(("Value" is not null));
insert into "Environment" values ('Test') on conflict do nothing; --update other environment databases value manually


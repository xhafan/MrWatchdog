alter table "WatchdogAlert" add "UserId" int8 not null;

alter table "WatchdogAlert" 
    add constraint "FK_WatchdogAlert_User"
    foreign key ("UserId") 
    references "User";

create index "WatchdogAlert_User_idx" on "WatchdogAlert" ("UserId");

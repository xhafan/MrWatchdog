alter table "Watchdog" add "UserId" int8 not null;

alter table "Watchdog" 
    add constraint "FK_Watchdog_User"
    foreign key ("UserId") 
    references "User";

create index "Watchdog_User_idx" on "Watchdog" ("UserId");

create table "LoginToken" (
    "Id" int8 not null,
   "Version" int8 not null,
   "Guid" uuid not null,
   "Token" text not null,
   "Confirmed" boolean not null,
   "Used" boolean not null,
   primary key ("Id")
);

create unique index "LoginToken_Guid_key" on "LoginToken" ("Guid");

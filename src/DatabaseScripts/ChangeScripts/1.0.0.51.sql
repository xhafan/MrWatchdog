create table "MailServerRateLimiting" (
    "Id" int8 not null,
   "MailServerName" text not null,
   "LastRateLimitedOn" timestamp not null,
   primary key ("Id")
);

create unique index "MailServerRateLimiting_MailServerName_key" on "MailServerRateLimiting" ("MailServerName");
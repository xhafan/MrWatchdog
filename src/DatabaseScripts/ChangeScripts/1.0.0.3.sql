create index "WatchdogAlert_WatchdogId_idx" on "WatchdogAlert" ("WatchdogId");
create index "WatchdogAlert_UserId_idx" on "WatchdogAlert" ("UserId");
create index "WatchdogWebPage_WatchdogId_idx" on "WatchdogWebPage" ("WatchdogId");

create table "Job" (
    "Id" int8 not null,
   "Version" int8 not null,
   "InputData" jsonb not null,
   "Guid" uuid not null,
   "CreatedOn" timestamp not null,
   "CompletedOn" timestamp,
   "Type" text not null,
   "Kind" char(1) not null,
   "NumberOfHandlingAttempts" int4 not null,
   primary key ("Id")
);

create table "JobAggregateRootEntity" (
    "Id" int8 not null,
   "Version" int8 not null,
   "JobId" int8 not null,
   "AggregateRootEntityName" text not null,
   "AggregateRootEntityId" int8 not null,
   primary key ("Id")
);

create table "JobHandlingAttempt" (
    "Id" int8 not null,
   "Version" int8 not null,
   "JobId" int8 not null,
   "StartedOn" timestamp not null,
   "EndedOn" timestamp,
   "Exception" text,
   primary key ("Id")
);

alter table "JobAggregateRootEntity" 
    add constraint "FK_JobAggregateRootEntity_Job" 
    foreign key ("JobId") 
    references "Job";

alter table "JobHandlingAttempt" 
    add constraint "FK_JobHandlingAttempt_Job" 
    foreign key ("JobId") 
    references "Job";

create unique index "Job_Guid_key" ON "Job" ("Guid");
create index "JobAggregateRootEntity_JobId_idx" on "JobAggregateRootEntity" ("JobId");
create index "JobHandlingAttempt_JobId_idx" on "JobHandlingAttempt" ("JobId");

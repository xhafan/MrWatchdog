ALTER TABLE "JobAggregateRootEntity" RENAME TO "JobAffectedEntity"; 
ALTER TABLE "JobAffectedEntity" RENAME COLUMN "AggregateRootEntityName" TO "EntityName";
ALTER TABLE "JobAffectedEntity" RENAME COLUMN "AggregateRootEntityId" TO "EntityId";

alter table "JobAffectedEntity" add "IsCreated" bool not null default(false);
alter table "JobAffectedEntity" alter column "IsCreated" drop default;
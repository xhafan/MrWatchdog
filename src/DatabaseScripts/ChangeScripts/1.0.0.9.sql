alter table "Job" add "RelatedCommandJobId" int8;

alter table "Job" 
    add constraint "FK_Job_RelatedCommandJob"
    foreign key ("RelatedCommandJobId") 
    references "Job";

create index "Job_RelatedCommandJobId_idx" on "Job" ("RelatedCommandJobId");
create table "UserCompleteOnboarding" (
    "UserId" int8 not null,
   "OnboardingIdentifier" text not null,
   primary key ("UserId", "OnboardingIdentifier")
);

alter table "UserCompleteOnboarding" 
    add constraint "FK_UserCompleteOnboarding_User" 
    foreign key ("UserId") 
    references "User";

create index "UserCompleteOnboarding_UserId_idx" on "UserCompleteOnboarding" ("UserId");

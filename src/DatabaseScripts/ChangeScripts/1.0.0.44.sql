CREATE TABLE IF NOT EXISTS "Logs" (
    "TimeStamp" timestamp NULL,
    "Message" text NULL,
    "MessageTemplate" text NULL,
    "Level" int4 NULL,
    "Exception" text NULL,
    "Properties" jsonb NULL
);

CREATE INDEX "Logs_TimeStamp_idx" ON "Logs" ("TimeStamp" DESC);
CREATE UNLOGGED TABLE "cache" (
    id varchar(449) COLLATE "C" NOT NULL,
    value bytea NOT NULL,
    expiresattime timestamp NOT NULL,
    slidingexpirationinseconds int8 NULL,
    absoluteexpiration timestamp NULL,
    CONSTRAINT cache_pkey PRIMARY KEY (id)
);
CREATE INDEX ix_expiresattime ON "cache" USING btree (expiresattime) WITH (deduplicate_items='true');
#!/bin/sh
set -eux

STAGING_VERSION=$(kamal app version -d staging-local -q | tail -n 2 | head -n 1)

# Deploy to production-local (pull staging image + deploy it)
kamal deploy -d production-local -P --version=$STAGING_VERSION

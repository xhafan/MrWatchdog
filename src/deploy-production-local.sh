#!/bin/sh
set -eux

STAGING_VERSION=$(kamal app version -d staging-local)

# Deploy to production-local (pull staging image + deploy it)
kamal deploy -d production-local -P --version=$STAGING_VERSION

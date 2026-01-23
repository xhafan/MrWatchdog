#!/bin/sh
set -eux

STAGING_VERSION=$(kamal app version -d staging-local)

# Deploy to production-hetzner (pull staging image + deploy it)
kamal deploy -d production-hetzner -P --version=$STAGING_VERSION

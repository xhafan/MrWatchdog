#!/bin/sh
set -eux

STAGING_VERSION=$(kamal app version -d staging-local -q | tail -n 2 | head -n 1)

# Deploy to production-hetzner (pull staging image + deploy it)
kamal deploy -d production-hetzner -P --version=$STAGING_VERSION

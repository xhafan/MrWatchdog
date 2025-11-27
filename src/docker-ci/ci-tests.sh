#!/bin/sh
set -eu

SECRETS_FILE="$HOME/.kamal/mrwatchdog_secrets_common.json"
export ReCaptcha__SiteKey=$(jq -r '.ReCaptcha__SiteKey' "$SECRETS_FILE")
export ReCaptcha__SecretKey=$(jq -r '.ReCaptcha__SecretKey' "$SECRETS_FILE")
export Authentication__Google__ClientSecret=$(jq -r '.Authentication__Google__ClientSecret' "$SECRETS_FILE")
export Authentication__Google__ClientId=$(jq -r '.Authentication__Google__ClientId' "$SECRETS_FILE")

# Ensure results dir exists
mkdir docker-ci/test-results

echo "==> Building test image..."
docker compose -f docker-ci/docker-compose.tests.yml build

echo "==> Running tests..."
set +e
docker compose -f docker-ci/docker-compose.tests.yml up --abort-on-container-exit --exit-code-from tests
STATUS=$?
set -e

echo "==> Cleaning up..."
docker compose -f docker-ci/docker-compose.tests.yml down -v

if [ "$STATUS" -eq 0 ]; then
    echo "==> Tests passed"
else
    echo "==> Tests failed (exit code $STATUS)"
fi

exit $STATUS

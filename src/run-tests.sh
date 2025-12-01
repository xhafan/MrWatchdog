#!/bin/sh
set -eux

DIRTY_MODE=false

while [ $# -gt 0 ]; do
  case "$1" in
    --dirty)
      DIRTY_MODE=true
      shift
      ;;
    *)
      echo "Unknown option: $1"
      exit 1
  esac
done

if [ "$DIRTY_MODE" = false ]; then
    if [ -z "${KAMAL_VERSION:-}" ]; then
        echo "Error: KAMAL_VERSION is not set. Set it manually (it's set within Kamal deployment), or add parameter --dirty"
        exit 1
    fi
fi


SECRETS_FILE="$HOME/.kamal/mrwatchdog_secrets_common.json"
export ReCaptcha__SiteKey=$(jq -r '.ReCaptcha__SiteKey' "$SECRETS_FILE")
export ReCaptcha__SecretKey=$(jq -r '.ReCaptcha__SecretKey' "$SECRETS_FILE")
export Authentication__Google__ClientSecret=$(jq -r '.Authentication__Google__ClientSecret' "$SECRETS_FILE")
export Authentication__Google__ClientId=$(jq -r '.Authentication__Google__ClientId' "$SECRETS_FILE")

if [ "$DIRTY_MODE" = true ]; then
    echo "==> Running in DIRTY mode (no clone, using working directory)"
    BUILD_ROOT="$PWD"
else
    echo "==> Creating temp clean clone"
    mkdir -p /tmp/test-clones
    BUILD_DIR=$(mktemp -d /tmp/test-clones/XXXXXXXX)
    REPO_ROOT=$(git -C "$PWD" rev-parse --show-toplevel)

    # Mimicking Kamal docker build process to create a clean and consistent Docker build context and to isolate the code being built.
    git -C $BUILD_DIR clone $REPO_ROOT --recurse-submodules
    CLONED_DIR="$(ls -1 "$BUILD_DIR" | head -n 1)"
    git -C $BUILD_DIR/$CLONED_DIR remote set-url origin $REPO_ROOT
    git -C $BUILD_DIR/$CLONED_DIR fetch origin
    git -C $BUILD_DIR/$CLONED_DIR reset --hard $KAMAL_VERSION
    git -C $BUILD_DIR/$CLONED_DIR clean -fdx
    git -C $BUILD_DIR/$CLONED_DIR submodule update --init
    BUILD_ROOT="$BUILD_DIR/$CLONED_DIR/src"
fi

echo "==> BUILD_ROOT $BUILD_ROOT"

echo "==> Building test image..."
docker compose -f $BUILD_ROOT/docker-compose.tests.yml build

echo "==> Running tests..."
set +e
docker compose -f $BUILD_ROOT/docker-compose.tests.yml up --abort-on-container-exit --exit-code-from tests
STATUS=$?
set -e

echo "==> Cleaning up..."
docker compose -f $BUILD_ROOT/docker-compose.tests.yml down -v

if [ "$DIRTY_MODE" = false ]; then
    rm -rf "$BUILD_DIR"
    echo "==> Removed temporary build directory: $BUILD_DIR"
fi

if [ "$STATUS" -eq 0 ]; then
    echo "==> Tests passed"
else
    echo "==> Tests failed (exit code $STATUS)"
fi

exit $STATUS

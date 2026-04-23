#!/usr/bin/env sh
# Sync the CoreBackend / CoreWeb subtrees with their upstream repos.

set -eu

usage() {
    echo "Usage: $0 <pull|push> <corebackend|coreweb>" >&2
    exit 1
}

[ $# -eq 2 ] || usage

case "$2" in
    corebackend)
        PREFIX=src/Libraries/CoreBackend
        URL=git@github.com:xhafan/CoreBackend.git
        ;;
    coreweb)
        PREFIX=src/Libraries/CoreWeb
        URL=git@github.com:xhafan/CoreWeb.git
        ;;
    *) usage ;;
esac

# Refresh the index to avoid spurious "working tree has modifications" from
# stat-dirty files (common on WSL reading /mnt/c). Harmless when clean.
git update-index --refresh >/dev/null || true

case "$1" in
    pull) git subtree pull --prefix="$PREFIX" "$URL" main --squash ;;
    push) git subtree push --prefix="$PREFIX" "$URL" main ;;
    *) usage ;;
esac
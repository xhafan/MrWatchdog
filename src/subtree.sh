#!/usr/bin/env sh
# Sync the CoreBackend / CoreWeb subtrees with their upstream repos.
# Usage: ./src/subtree.sh <pull|push> <corebackend|coreweb>
set -eu

usage() {
    echo "Usage: $0 <pull|push> <corebackend|coreweb>" >&2
    exit 1
}

[ $# -eq 2 ] || usage

case "$2" in
    corebackend)
        PREFIX=src/Libraries/CoreBackend
        URL=https://github.com/xhafan/CoreBackend.git
        ;;
    coreweb)
        PREFIX=src/Libraries/CoreWeb
        URL=https://github.com/xhafan/CoreWeb.git
        ;;
    *) usage ;;
esac

case "$1" in
    pull) git subtree pull --prefix="$PREFIX" "$URL" main --squash ;;
    push) git subtree push --prefix="$PREFIX" "$URL" main ;;
    *) usage ;;
esac

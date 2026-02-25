#!/usr/bin/env bash
set -euo pipefail

log() { echo "[start] $*"; }
err() {
  echo "[error] $*" >&2
  exit 1
}

# check docker
if ! command -v docker &>/dev/null; then
  err "docker is not installed. install it from https://docs.docker.com/get-docker/"
fi
log "docker found: $(docker --version)"

# check docker compose (v2 plugin or standalone v1)
if docker compose version &>/dev/null 2>&1; then
  COMPOSE="docker compose"
elif command -v docker-compose &>/dev/null; then
  COMPOSE="docker-compose"
else
  err "docker compose is not installed. install it from https://docs.docker.com/compose/install/"
fi
log "docker compose found: $($COMPOSE version)"

# move to project root (same dir as this script)
cd "$(dirname "$0")"

log "building and starting services..."
$COMPOSE up --build -d db backend

log "waiting for tests container to complete..."
$COMPOSE run --rm tests
TEST_EXIT=$?

if [ $TEST_EXIT -ne 0 ]; then
  err "tests failed (exit $TEST_EXIT). frontend will not be started."
fi

log "tests passed. starting frontend..."
$COMPOSE up --build -d frontend

log "all services running."
log "  backend  -> http://localhost:5291"
log "  swagger  -> http://localhost:5291/swagger"
log "  frontend -> http://localhost:4200"

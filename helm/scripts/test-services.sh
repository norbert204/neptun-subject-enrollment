#!/usr/bin/env bash
# Test helper for deployed services in this repo
# Usage:
#   NAMESPACE=my-namespace RELEASE_PREFIX=my-release ./helm/scripts/test-services.sh
# Defaults:
#   NAMESPACE=default
#   RELEASE_PREFIX=<release prefix used by Helm release names; if empty we assume services are named exactly as in the chart>

set -euo pipefail

NAMESPACE=${NAMESPACE:-neptun}
RELEASE_PREFIX=${RELEASE_PREFIX:-}

# Helper to compute service name: if RELEASE_PREFIX is set, use "${RELEASE_PREFIX}-${chart-name}",
# otherwise use the chart name directly.
svc_name() {
  chart_name=$1
  if [ -n "$RELEASE_PREFIX" ]; then
    echo "${RELEASE_PREFIX}-${chart_name}"
  else
    echo "$chart_name"
  fi
}

# Ports (local:remote)
declare -A PORTS
PORTS[caching-service_local]=5100:80
PORTS[database-service_local]=5000:80
PORTS[auth-service_local]=5200:80
PORTS[subject-service_local]=5300:80
PORTS[db-operations-service_local]=5400:80

PIDS=()

start_port_forward() {
  local service=$1
  local mapping=$2
  echo "Starting port-forward for service: $service -> $mapping"
  kubectl -n "$NAMESPACE" port-forward svc/"$service" $mapping >/dev/null 2>&1 &
  pid=$!
  PIDS+=("$pid")
  # give it a moment
  sleep 0.6
}

cleanup() {
  echo "Cleaning up port-forwards..."
  for pid in "${PIDS[@]:-}"; do
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" || true
    fi
  done
}
trap cleanup EXIT

# Start port-forwards
start_port_forward "$(svc_name caching-service)" "${PORTS[caching-service_local]}"
start_port_forward "$(svc_name database-service)" "${PORTS[database-service_local]}"
start_port_forward "$(svc_name auth-service)" "${PORTS[auth-service_local]}"
start_port_forward "$(svc_name subject-service)" "${PORTS[subject-service_local]}"
start_port_forward "$(svc_name db-operations-service)" "${PORTS[db-operations-service_local]}"

# Local endpoints
CACHING_HOST=localhost:5100
DATABASE_HOST=localhost:5000
AUTH_HOST=localhost:5200
SUBJECT_HOST=localhost:5300
DBOPS_HOST=localhost:5400

echo
echo "=== Quick HTTP root checks ==="
set +e
curl -k -sS -o /dev/null -w "%{http_code} %url_effective\n" http://$CACHING_HOST/ || true
curl -k -sS -o /dev/null -w "%{http_code} %url_effective\n" http://$DATABASE_HOST/ || true
curl -k -sS -o /dev/null -w "%{http_code} %url_effective\n" http://$AUTH_HOST/ || true
curl -k -sS -o /dev/null -w "%{http_code} %url_effective\n" http://$SUBJECT_HOST/ || true
curl -k -sS -o /dev/null -w "%{http_code} %url_effective\n" http://$DBOPS_HOST/ || true
set -e

echo
echo "=== Caching service REST tests (HTTP-transcoded gRPC) ==="
# Set a key
curl -k -s -X POST http://$CACHING_HOST/api/redis -H "Content-Type: application/json" -d '{"key":"testkey","data":"hello","expiry_seconds":60}' || true
# Get the key
curl -k -s http://$CACHING_HOST/api/redis/testkey || true
# Delete the key
curl -k -s -X DELETE http://$CACHING_HOST/api/redis/testkey || true

echo
echo "=== gRPC tests with grpcurl (plaintext) ==="
if ! command -v grpcurl >/dev/null 2>&1; then
  echo "grpcurl not found in PATH — skipping grpcurl tests. Install grpcurl to enable gRPC checks."
else
  echo "-- Caching (Redis) service --"
  grpcurl -plaintext -d '{"key":"testkey"}' $CACHING_HOST redis.RedisService/GetValue || true
  grpcurl -plaintext -d '{"key":"testkey","data":"from-grpc","expiry_seconds":60}' $CACHING_HOST redis.RedisService/SetValue || true

  echo "-- Database (User/Subject/Course) services --"
  grpcurl -plaintext -d '{}' $DATABASE_HOST user.UserService/ListUsers || true
  grpcurl -plaintext -d '{}' $DATABASE_HOST subject.SubjectService/ListSubjects || true
  grpcurl -plaintext -d '{}' $DATABASE_HOST course.CourseService/ListCourses || true

  echo "-- Auth service --"
  grpcurl -plaintext -d '{"neptunCode":"ABC123","password":"pass"}' $AUTH_HOST authservice.AuthService/Login || true
fi

echo
echo "Tests finished. Port-forwards will be stopped." 
# cleanup happens via trap

exit 0

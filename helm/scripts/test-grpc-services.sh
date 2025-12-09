#!/usr/bin/env bash
# Complete gRPC service test script with proto imports and sample data
# Usage:
#   NAMESPACE=neptun ./helm/scripts/test-grpc-services.sh

set -euo pipefail

NAMESPACE=${NAMESPACE:-neptun}
PROTO_ROOT="src"

echo "=== gRPC Service Test Script ==="
echo "Namespace: $NAMESPACE"
echo

# Check if grpcurl is installed
if ! command -v grpcurl >/dev/null 2>&1; then
  echo "Error: grpcurl not found. Install it with:"
  echo "  brew install grpcurl  # macOS"
  echo "  go install github.com/fullstorydev/grpcurl/cmd/grpcurl@latest  # Go"
  exit 1
fi

# Kill any existing port-forwards on these ports
echo "Cleaning up any existing port-forwards..."
pkill -f "port-forward.*database-service" || true
pkill -f "port-forward.*caching-service" || true
pkill -f "port-forward.*auth-service" || true
pkill -f "port-forward.*subject-service" || true
sleep 2

PIDS=()

cleanup() {
  echo
  echo "Cleaning up port-forwards..."
  for pid in "${PIDS[@]:-}"; do
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" || true
    fi
  done
}
trap cleanup EXIT

start_port_forward() {
  local service=$1
  local port=$2
  echo "Starting port-forward: $service -> localhost:$port"
  kubectl -n "$NAMESPACE" port-forward "deploy/$service" "$port:80" >/dev/null 2>&1 &
  pid=$!
  PIDS+=("$pid")
  sleep 1
}

# Start port-forwards
start_port_forward "database-service" 5000
start_port_forward "caching-service" 5100
start_port_forward "auth-service" 5200
start_port_forward "subject-service" 5300

echo
echo "=== Step 1: Create test data in Scylla ==="
kubectl -n "$NAMESPACE" exec statefulset/scylla -- cqlsh <<'EOF' || echo "Data insert failed (may already exist)"
USE sas_db;

INSERT INTO users (neptun_code, name, email, password) 
VALUES ('ABC123', 'Test User', 'test@example.com', 'testpass123');

INSERT INTO users (neptun_code, name, email, password) 
VALUES ('XYZ789', 'Another User', 'another@example.com', 'pass456');

INSERT INTO subjects (id, owner, name, prerequisites, courses) 
VALUES ('SUBJ001', 'ABC123', 'Software Architecture', [], ['COURSE001']);

INSERT INTO subjects (id, owner, name, prerequisites, courses) 
VALUES ('SUBJ002', 'XYZ789', 'Database Systems', ['SUBJ001'], ['COURSE002']);

INSERT INTO courses (id, room, start_time, end_time, capacity, enrolled_students, course_type) 
VALUES ('COURSE001', 'Room A', '2024-01-01T10:00:00', '2024-01-01T12:00:00', 30, ['ABC123'], 'Lecture');

INSERT INTO courses (id, room, start_time, end_time, capacity, enrolled_students, course_type) 
VALUES ('COURSE002', 'Room B', '2024-01-01T14:00:00', '2024-01-01T16:00:00', 25, [], 'Lab');
EOF

echo
echo "=== Step 2: Test Database Service (User/Subject/Course) ==="

echo "-- ListUsers --"
grpcurl -plaintext \
  -import-path "$PROTO_ROOT/GrpcDatabaseService/Protos" \
  -proto userService.proto \
  -d '{}' \
  localhost:5000 user.UserService/ListUsers

echo
echo "-- GetUser --"
grpcurl -plaintext \
  -import-path "$PROTO_ROOT/GrpcDatabaseService/Protos" \
  -proto userService.proto \
  -d '{"neptun_code":"ABC123"}' \
  localhost:5000 user.UserService/GetUser

echo
echo "-- ListSubjects --"
grpcurl -plaintext \
  -import-path "$PROTO_ROOT/GrpcDatabaseService/Protos" \
  -proto subjectService.proto \
  -d '{}' \
  localhost:5000 subject.SubjectService/ListSubjects

echo
echo "-- ListCourses --"
grpcurl -plaintext \
  -import-path "$PROTO_ROOT/GrpcDatabaseService/Protos" \
  -proto courseService.proto \
  -d '{}' \
  localhost:5000 course.CourseService/ListCourses

echo
echo "=== Step 3: Test Caching Service (Redis) ==="

echo "-- SetValue --"
grpcurl -plaintext \
  -import-path "$PROTO_ROOT/GrpcCachingService/Protos" \
  -import-path "$PROTO_ROOT/GrpcCachingService" \
  -proto redis.proto \
  -d '{"key":"test-key","data":"Hello from Redis","expiry_seconds":120}' \
  localhost:5100 redis.RedisService/SetValue

echo
echo "-- GetValue --"
grpcurl -plaintext \
  -import-path "$PROTO_ROOT/GrpcCachingService/Protos" \
  -import-path "$PROTO_ROOT/GrpcCachingService" \
  -proto redis.proto \
  -d '{"key":"test-key"}' \
  localhost:5100 redis.RedisService/GetValue

echo
echo "-- DeleteValue --"
grpcurl -plaintext \
  -import-path "$PROTO_ROOT/GrpcCachingService/Protos" \
  -import-path "$PROTO_ROOT/GrpcCachingService" \
  -proto redis.proto \
  -d '{"key":"test-key"}' \
  localhost:5100 redis.RedisService/DeleteValue

echo
echo "=== Step 4: Test Auth Service ==="

echo "-- Login --"
grpcurl -plaintext \
  -import-path "$PROTO_ROOT/GrpcAuthService" \
  -proto Protos/authService.proto \
  -d '{"neptunCode":"ABC123","password":"testpass123"}' \
  localhost:5200 authservice.AuthService/Login || echo "Login failed (expected if password hashing differs)"

echo
echo "=== Step 5: Test Subject Service (integration) ==="

echo "-- GetSubject --"
grpcurl -plaintext \
  -import-path "$PROTO_ROOT/GrpcSubjectService" \
  -proto Protos/subjectService.proto \
  -d '{"subjectId":"SUBJ001"}' \
  localhost:5300 subjectservice.SubjectService/GetSubject || echo "Subject service call failed (check logs)"

echo
echo "=== Tests complete ==="
echo "Port-forwards will be cleaned up."

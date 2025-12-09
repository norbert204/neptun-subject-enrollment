#!/usr/bin/env bash
# Test Gateway REST API endpoints
# This script tests the Gateway service which calls downstream gRPC services
# Usage: ./helm/scripts/test-gateway.sh

set -euo pipefail

NAMESPACE=${NAMESPACE:-neptun}
GATEWAY_SERVICE=${GATEWAY_SERVICE:-gateway}
GATEWAY_PORT=${GATEWAY_PORT:-8080}

echo "=== Gateway API Test Script ==="
echo "Testing Gateway service in namespace: $NAMESPACE"
echo ""

# Start port-forward in background
echo "Starting port-forward to gateway service..."
kubectl port-forward -n "$NAMESPACE" "svc/$GATEWAY_SERVICE" "$GATEWAY_PORT:80" > /dev/null 2>&1 &
PF_PID=$!

# Cleanup function
cleanup() {
  echo ""
  echo "Cleaning up port-forward (PID: $PF_PID)..."
  kill $PF_PID 2>/dev/null || true
}
trap cleanup EXIT

# Wait for port-forward to be ready
sleep 3
echo "Port-forward ready on localhost:$GATEWAY_PORT"
echo ""

BASE_URL="http://localhost:$GATEWAY_PORT"

# Test data
TEST_USER_NEPTUN="ABC123"
TEST_USER_NAME="Test User"
TEST_USER_EMAIL="test@example.com"
TEST_USER_PASSWORD="password123"

TEST_SUBJECT_ID="subject-001"
TEST_SUBJECT_NAME="Software Architecture"
TEST_SUBJECT_OWNER="Dr. Smith"

TEST_COURSE_ID="course-001"
TEST_COURSE_ROOM="IB028"
TEST_COURSE_START="2024-01-10T10:00:00Z"
TEST_COURSE_END="2024-01-10T12:00:00Z"
TEST_COURSE_CAPACITY=30
TEST_COURSE_TYPE="Lecture"

echo "================================"
echo "1. ADMIN USER OPERATIONS"
echo "================================"

echo ""
echo ">>> Create User (POST /admin/user)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X POST "$BASE_URL/admin/user" \
  -H "Content-Type: application/json" \
  -d "{
    \"neptunCode\": \"$TEST_USER_NEPTUN\",
    \"name\": \"$TEST_USER_NAME\",
    \"email\": \"$TEST_USER_EMAIL\",
    \"password\": \"$TEST_USER_PASSWORD\"
  }" | { read -r status; echo "HTTP Status: $status"; cat /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo ">>> List All Users (GET /admin/user)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X GET "$BASE_URL/admin/user" \
  -H "Accept: application/json" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo ">>> Get User by Neptun Code (GET /admin/user/{neptunCode})"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X GET "$BASE_URL/admin/user/$TEST_USER_NEPTUN" \
  -H "Accept: application/json" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo "================================"
echo "2. ADMIN COURSE OPERATIONS"
echo "================================"

echo ""
echo ">>> Create Course (POST /admin/course)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X POST "$BASE_URL/admin/course" \
  -H "Content-Type: application/json" \
  -d "{
    \"id\": \"$TEST_COURSE_ID\",
    \"room\": \"$TEST_COURSE_ROOM\",
    \"startTime\": \"$TEST_COURSE_START\",
    \"endTime\": \"$TEST_COURSE_END\",
    \"capacity\": $TEST_COURSE_CAPACITY,
    \"courseType\": \"$TEST_COURSE_TYPE\"
  }" | { read -r status; echo "HTTP Status: $status"; cat /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo ">>> List All Courses (GET /admin/course)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X GET "$BASE_URL/admin/course" \
  -H "Accept: application/json" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo ">>> Get Course by ID (GET /admin/course/{courseId})"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X GET "$BASE_URL/admin/course/$TEST_COURSE_ID" \
  -H "Accept: application/json" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo "================================"
echo "3. ADMIN SUBJECT OPERATIONS"
echo "================================"

echo ""
echo ">>> Create Subject (POST /admin/subject)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X POST "$BASE_URL/admin/subject" \
  -H "Content-Type: application/json" \
  -d "{
    \"id\": \"$TEST_SUBJECT_ID\",
    \"name\": \"$TEST_SUBJECT_NAME\",
    \"owner\": \"$TEST_SUBJECT_OWNER\",
    \"courses\": [\"$TEST_COURSE_ID\"],
    \"prerequisites\": []
  }" | { read -r status; echo "HTTP Status: $status"; cat /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo ">>> List All Subjects (GET /admin/subject)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X GET "$BASE_URL/admin/subject" \
  -H "Accept: application/json" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo ">>> Get Subject by ID (GET /admin/subject/{subjectId})"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X GET "$BASE_URL/admin/subject/$TEST_SUBJECT_ID" \
  -H "Accept: application/json" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo "================================"
echo "4. AUTH OPERATIONS"
echo "================================"

echo ""
echo ">>> Login (POST /auth/login)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X POST "$BASE_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{
    \"neptunCode\": \"$TEST_USER_NEPTUN\",
    \"password\": \"$TEST_USER_PASSWORD\"
  }" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"

# Extract tokens if login was successful
ACCESS_TOKEN=$(jq -r '.accessToken' /tmp/gateway_body.json 2>/dev/null || echo "")
REFRESH_TOKEN=$(jq -r '.refreshToken' /tmp/gateway_body.json 2>/dev/null || echo "")

if [ -n "$ACCESS_TOKEN" ] && [ "$ACCESS_TOKEN" != "null" ]; then
  echo ""
  echo ">>> Refresh Token (POST /auth/refresh-token)"
  curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X POST "$BASE_URL/auth/refresh-token" \
    -H "Content-Type: application/json" \
    -d "{
      \"accessToken\": \"$ACCESS_TOKEN\",
      \"refreshToken\": \"$REFRESH_TOKEN\"
    }" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"
else
  echo "Skipping refresh token test (login failed or returned no token)"
fi

echo ""
echo "================================"
echo "5. SUBJECT OPERATIONS (Student)"
echo "================================"

echo ""
echo ">>> Initialize Enrollment Period (POST /admin/start-enrollment-period)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X POST "$BASE_URL/admin/start-enrollment-period" \
  -H "Content-Type: application/json" | { read -r status; echo "HTTP Status: $status"; cat /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo ">>> Get Eligible Courses for Student (GET /subject/student/{studentId}/eligible-courses)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X GET "$BASE_URL/subject/student/$TEST_USER_NEPTUN/eligible-courses" \
  -H "Accept: application/json" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo ">>> Enroll to Course (POST /subject/enroll-to-course)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X POST "$BASE_URL/subject/enroll-to-course" \
  -H "Content-Type: application/json" \
  -d "{
    \"studentId\": \"$TEST_USER_NEPTUN\",
    \"courseId\": \"$TEST_COURSE_ID\"
  }" | { read -r status; echo "HTTP Status: $status"; cat /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo ">>> Get Enrolled Courses for Student (GET /subject/student/{studentId}/enrolled)"
curl -s -o /tmp/gateway_body.json -w "%{http_code}" -X GET "$BASE_URL/subject/student/$TEST_USER_NEPTUN/enrolled" \
  -H "Accept: application/json" | { read -r status; echo "HTTP Status: $status"; jq '.' /tmp/gateway_body.json; } || echo "Request failed"

echo ""
echo "================================"
echo "6. CLEANUP TEST DATA"
echo "================================"

echo ""
echo ">>> Delete Course (DELETE /admin/course/{courseId})"
curl -X DELETE "$BASE_URL/admin/course/$TEST_COURSE_ID" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s || echo "Request failed"

echo ""
echo ">>> Delete Subject (DELETE /admin/subject/{subjectId})"
curl -X DELETE "$BASE_URL/admin/subject/$TEST_SUBJECT_ID" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s || echo "Request failed"

echo ""
echo ">>> Delete User (DELETE /admin/user/{neptunCode})"
curl -X DELETE "$BASE_URL/admin/user/$TEST_USER_NEPTUN" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s || echo "Request failed"

echo ""
echo "=== Gateway API Tests Complete ==="
echo ""
echo "Summary of tested endpoints:"
echo "  - Admin: User CRUD, Course CRUD, Subject CRUD"
echo "  - Auth: Login, Refresh Token"
echo "  - Subject: Eligible Courses, Enroll, Enrolled Courses"
echo ""
echo "Check HTTP status codes above to verify connectivity between services."
echo "Expected: 200 OK for successful operations, 400/500 for errors."

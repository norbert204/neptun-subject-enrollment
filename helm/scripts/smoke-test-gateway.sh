#!/usr/bin/env bash
# Smoke test generator for the Gateway
# - Runs a mixed workload hitting Admin & Subject endpoints
# - Default: 120s duration, 20 workers
# - Supports two modes:
#     mode=local  -> hits frontend nginx (use --url to point to NodePort)
#     mode=cluster -> hits in-cluster gateway service directly (http://gateway:80)
# - Optionally performs a deletion/cleanup phase for created resources when -C is set
#
# Usage examples:
#  # Run locally against frontend NodePort for 2 minutes (default)
#  bash helm/scripts/smoke-test-gateway.sh -m local -u http://10.5.0.2:30080 -p DEMO -d 120 -w 30
#
#  # Run inside cluster mode (hits gateway service directly)
#  bash helm/scripts/smoke-test-gateway.sh -m cluster -p DEMO -d 120 -w 40
#
#  # Run and cleanup created resources at the end
#  bash helm/scripts/smoke-test-gateway.sh -m cluster -p DEMO -d 120 -w 40 -C
#
set -eu

usage() {
  cat <<EOF
Usage: $0 [-m local|cluster] [-u BASE_URL] [-p PREFIX] [-d DURATION] [-w WORKERS] [-C] [-H 'Header: val']

Options:
  -m MODE       'local' (default) or 'cluster' (in-cluster gateway). In local mode you must provide -u BASE_URL (e.g. http://10.5.0.2:30080)
  -u BASE_URL   Base URL for local mode (no trailing slash). Example: http://10.5.0.2:30080
  -p PREFIX     Prefix used when creating resources (default SMK)
  -d DURATION   Duration in seconds to run the load phase (default 120)
  -w WORKERS    Number of parallel workers (default 20)
  -C            Cleanup (delete) created resources at the end
  -H HEADER     Extra HTTP header to include (can be repeated)
  -h            Show help

This script will create users and subjects (with courses) and exercise read/write endpoints
to generate load against the Gateway. When -C is given the script attempts to delete created
users and subjects at the end.
EOF
}

MODE=local
BASE_URL=""
PREFIX=SMK
DURATION=120
WORKERS=20
CLEANUP=0
HEADERS=()

while getopts ":m:u:p:d:w:CH:h" opt; do
  case ${opt} in
    m) MODE=${OPTARG} ;; 
    u) BASE_URL=${OPTARG} ;; 
    p) PREFIX=${OPTARG} ;; 
    d) DURATION=${OPTARG} ;; 
    w) WORKERS=${OPTARG} ;; 
    C) CLEANUP=1 ;; 
    H) HEADERS+=(-H "${OPTARG}") ;; 
    h) usage; exit 0 ;;
    \?) echo "Invalid option: -${OPTARG}" >&2; usage; exit 1 ;;
  esac
done

if [ "$MODE" != "local" ] && [ "$MODE" != "cluster" ]; then
  echo "Invalid mode: $MODE" >&2; usage; exit 1
fi

if [ "$MODE" = "local" ] && [ -z "$BASE_URL" ]; then
  echo "In local mode you must supply -u BASE_URL" >&2; usage; exit 1
fi

if ! command -v curl >/dev/null 2>&1; then
  echo "Error: curl is required." >&2; exit 2
fi

# Endpoint bases depending on mode
if [ "$MODE" = "cluster" ]; then
  ADMIN_BASE="http://gateway:80/Admin"
  SUBJECT_BASE="http://gateway:80/Subject"
else
  ADMIN_BASE="${BASE_URL}/api/Admin"
  SUBJECT_BASE="${BASE_URL}/api/Subject"
fi

echo "Smoke test starting: mode=$MODE admin=$ADMIN_BASE subject=$SUBJECT_BASE duration=${DURATION}s workers=${WORKERS} prefix=${PREFIX} cleanup=${CLEANUP}"

# Temp files to record created resources
USERS_FILE=$(mktemp /tmp/smk-users.XXXX)
SUBJECTS_FILE=$(mktemp /tmp/smk-subjects.XXXX)
COURSES_FILE=$(mktemp /tmp/smk-courses.XXXX)

cleanup_files() {
  rm -f "$USERS_FILE" "$SUBJECTS_FILE" "$COURSES_FILE"
}
trap cleanup_files EXIT

pick_random() {
  local f=$1
  if [ ! -s "$f" ]; then
    echo ""
    return
  fi
  awk 'BEGIN{srand();}{a[NR]=$0}END{print a[int(rand()*NR)+1]}' "$f"
}

append_unique() {
  local f=$1; shift
  local v=$1
  # append value, duplicates allowed
  printf "%s\n" "$v" >> "$f"
}

create_user() {
  local id=$1
  local payload
  payload=$(printf '{"NeptunCode":"%s","Name":"%s","Email":"%s","Password":"%s"}' "$id" "User $id" "$id@example.com" "P@ssw0rd123")
  http=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$ADMIN_BASE/user" "${HEADERS[@]}" -H "Content-Type: application/json" -d "$payload" ) || http=000
  echo "CREATE_USER $id -> $http"
  if [ "$http" -ge 200 ] && [ "$http" -lt 300 ]; then
    # wait until user is visible via GET (avoid race with downstream services)
    for i in 1 2 3 4 5; do
      status=$(curl -s -o /dev/null -w "%{http_code}" "$ADMIN_BASE/user/$id" "${HEADERS[@]}" || echo 000)
      if [ "$status" = "200" ]; then
        append_unique "$USERS_FILE" "$id"
        break
      fi
      sleep 0.2
    done
  fi
}

create_subject() {
  local id=$1
  local courseId="${id}-C1"
  local payload
  payload=$(printf '{"Id":"%s","Owner":"%s","Name":"%s","Courses":["%s"],"Prerequisites":[]}' "$id" "Owner $id" "Subject $id" "$courseId")
  http=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$ADMIN_BASE/subject" "${HEADERS[@]}" -H "Content-Type: application/json" -d "$payload" ) || http=000
  echo "CREATE_SUBJECT $id (course $courseId) -> $http"
  if [ "$http" -ge 200 ] && [ "$http" -lt 300 ]; then
    # wait until the course is visible via GET to avoid race conditions
    for i in 1 2 3 4 5; do
      status=$(curl -s -o /dev/null -w "%{http_code}" "$ADMIN_BASE/course/$courseId" "${HEADERS[@]}" || echo 000)
      if [ "$status" = "200" ]; then
        append_unique "$SUBJECTS_FILE" "$id"
        append_unique "$COURSES_FILE" "$courseId"
        break
      fi
      sleep 0.2
    done
  fi
}

get_users() {
  curl -s "${ADMIN_BASE}/user" "${HEADERS[@]}"
}

get_subjects() {
  curl -s "${ADMIN_BASE}/subject" "${HEADERS[@]}"
}

eligible_courses() {
  local student=$1
  curl -s "${SUBJECT_BASE}/student/${student}/eligible-courses" "${HEADERS[@]}"
}

enroll_course() {
  local student=$1; local course=$2
  payload=$(printf '{"StudentId":"%s","CourseId":"%s"}' "$student" "$course")
  TMP=$(mktemp)
  http=$(curl -s -o "$TMP" -w "%{http_code}" -X POST "$SUBJECT_BASE/enroll-to-course" "${HEADERS[@]}" -H "Content-Type: application/json" -d "$payload" ) || http=000
  if [ "$http" -ge 200 ] && [ "$http" -lt 300 ]; then
    echo "ENROLL ${student} -> ${course} -> $http"
  else
    echo "ENROLL ${student} -> ${course} -> $http"
    echo "  Response body (first 400 chars):"
    head -c 400 "$TMP" | sed -n '1,200p'
    echo "  (full body saved to $TMP)"
  fi
  # don't remove TMP so troubleshooting can inspect it if needed
}

start_enrollment() {
  http=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$ADMIN_BASE/start-enrollment-period" "${HEADERS[@]}" ) || http=000
  echo "START_ENROLLMENT -> $http"
}

delete_user() {
  local id=$1
  http=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$ADMIN_BASE/user/$id" "${HEADERS[@]}" ) || http=000
  echo "DELETE_USER $id -> $http"
}

delete_subject() {
  local id=$1
  http=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$ADMIN_BASE/subject/$id" "${HEADERS[@]}" ) || http=000
  echo "DELETE_SUBJECT $id -> $http"
}

# worker routine
worker() {
  local id=$1
  local end_time=$2
  while [ $(date +%s) -lt $end_time ]; do
    # pick random action
    r=$((RANDOM % 100))
    if [ $r -lt 10 ]; then
      # create user
      idx=$((RANDOM % 10000))
      neptun="${PREFIX}U$(printf '%04d' $idx)"
      create_user "$neptun"
    elif [ $r -lt 17 ]; then
      # create subject
      idx=$((RANDOM % 10000))
      sid="${PREFIX}S$(printf '%04d' $idx)"
      create_subject "$sid"
    elif [ $r -lt 35 ]; then
      # list users
      get_users >/dev/null
    elif [ $r -lt 55 ]; then
      # list subjects
      get_subjects >/dev/null
    elif [ $r -lt 75 ]; then
      # eligible courses for random student
      student=$(pick_random "$USERS_FILE")
      if [ -n "$student" ]; then
        eligible_courses "$student" >/dev/null
      fi
    else
      # enroll random student in random course
      student=$(pick_random "$USERS_FILE")
      course=$(pick_random "$COURSES_FILE")
      if [ -n "$student" ] && [ -n "$course" ]; then
        enroll_course "$student" "$course"
      fi
    fi
    # small random sleep to vary request timing
    sleep_time=$(awk -v s=0 -v e=0.2 'BEGIN{srand(); print s+rand()*(e-s)}')
    sleep $sleep_time
  done
}

END_TIME=$(( $(date +%s) + DURATION ))
PIDS=()
# ensure enrollment period is started so enroll calls succeed
start_enrollment >/dev/null || true
sleep 1
# pre-seed some users and subjects so workers have items to enroll to
echo "Pre-seeding baseline users and subjects..."
for idx in $(seq 1 20); do
  neptun="${PREFIX}U$(printf '%04d' $((1000 + idx)))"
  sid="${PREFIX}S$(printf '%04d' $((1000 + idx)))"
  create_user "$neptun"
  create_subject "$sid"
  # small pause to let creation propagate
  sleep 0.05
done
echo "Pre-seed complete."
END_TIME=$(( $(date +%s) + DURATION ))
for i in $(seq 1 $WORKERS); do
  worker $i $END_TIME &
  PIDS+=($!)
done

for pid in "${PIDS[@]}"; do
  wait $pid || true
done

echo "Load phase finished."

if [ $CLEANUP -eq 1 ]; then
  echo "Starting cleanup phase..."
  # delete users
  if [ -s "$USERS_FILE" ]; then
    sort -u "$USERS_FILE" | while read -r u; do
      delete_user "$u"
    done
  fi
  # delete subjects
  if [ -s "$SUBJECTS_FILE" ]; then
    sort -u "$SUBJECTS_FILE" | while read -r s; do
      delete_subject "$s"
    done
  fi
  echo "Cleanup phase finished."
else
  echo "Cleanup skipped (use -C to enable). Created resources are recorded in:"
  echo "  users -> $USERS_FILE"
  echo "  subjects -> $SUBJECTS_FILE"
  echo "  courses -> $COURSES_FILE"
fi

exit 0

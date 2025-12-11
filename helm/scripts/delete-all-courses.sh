#!/usr/bin/env bash
# Delete all courses returned by Gateway Admin API
# Usage: ./helm/scripts/delete-all-courses.sh [-n namespace] [-s gateway-service] [-p prefix] [-P port] [-y]
# -n namespace (default: neptun)
# -s gateway-service (default: gateway)
# -P local port for port-forward (default: 8080)
# -p prefix: if provided, only delete courses whose id starts with this prefix
# -y: skip confirmation prompt (dangerous)

set -euo pipefail

NAMESPACE=${NAMESPACE:-neptun}
GATEWAY_SERVICE=${GATEWAY_SERVICE:-gateway}
LOCAL_PORT=${GATEWAY_PORT:-8080}
PREFIX=""
ASSUME_YES=0

while getopts ":n:s:p:P:y" opt; do
  case ${opt} in
    n ) NAMESPACE=$OPTARG ;;
    s ) GATEWAY_SERVICE=$OPTARG ;;
    p ) PREFIX=$OPTARG ;;
    P ) LOCAL_PORT=$OPTARG ;;
    y ) ASSUME_YES=1 ;;
    \? ) echo "Usage: $0 [-n namespace] [-s gateway-service] [-p prefix] [-P port] [-y]"; exit 1 ;;
  esac
done

BASE_URL="http://localhost:${LOCAL_PORT}"

echo "Deleting courses via Gateway in namespace: ${NAMESPACE} (service: ${GATEWAY_SERVICE})"

echo "Starting port-forward to ${GATEWAY_SERVICE} service..."
kubectl port-forward -n "${NAMESPACE}" "svc/${GATEWAY_SERVICE}" "${LOCAL_PORT}:80" >/dev/null 2>&1 &
PF_PID=$!

cleanup() {
  echo "\nCleaning up port-forward (PID: ${PF_PID})..."
  kill ${PF_PID} 2>/dev/null || true
}
trap cleanup EXIT

# wait a moment for port-forward
sleep 2

# fetch all courses
TMP_JSON=$(mktemp)
HTTP_STATUS=$(curl -s -w "%{http_code}" -o "${TMP_JSON}" "${BASE_URL}/admin/course") || {
  echo "Failed to fetch courses from ${BASE_URL}/admin/course"; cat "${TMP_JSON}" || true; exit 1
}

if [ "${HTTP_STATUS}" != "200" ]; then
  echo "GET /admin/course returned HTTP ${HTTP_STATUS}"; cat "${TMP_JSON}" || true; exit 1
fi

# Extract possible id fields (handle several response shapes)
mapfile -t ITEMS < <(jq -r '
  def cand: (try .users catch null) // (try .data catch null) // (try .items catch null) // (try .courses catch null) // .;
  (cand | if type=="array" then .[] else . end)
  | if type=="object" then (.id // .Id // .courseId // .CourseId // .code // .Code)
    elif type=="string" then . else empty end
' "${TMP_JSON}") || { echo "Failed to parse JSON response with jq:"; cat "${TMP_JSON}"; exit 1; }

if [ ${#ITEMS[@]} -eq 0 ]; then
  echo "No courses found to delete."; exit 0
fi

echo "Found ${#ITEMS[@]} courses. Listing sample (first 10):"
for i in "${!ITEMS[@]}"; do
  if [ $i -lt 10 ]; then echo "  - ${ITEMS[$i]}"; fi
done

# apply prefix filter if provided
if [ -n "${PREFIX}" ]; then
  echo "Filtering courses by prefix: '${PREFIX}'"
  FILTERED=()
  for u in "${ITEMS[@]}"; do
    case "$u" in
      ${PREFIX}*) FILTERED+=("$u") ;;
    esac
  done
  ITEMS=("${FILTERED[@]}")
  echo "After filter: ${#ITEMS[@]} courses will be deleted."
fi

if [ ${#ITEMS[@]} -eq 0 ]; then
  echo "No courses matched filter. Exiting."; exit 0
fi

if [ ${ASSUME_YES} -eq 0 ]; then
  echo -n "Are you sure you want to DELETE ${#ITEMS[@]} courses? Type YES to confirm: "
  read -r CONFIRM
  if [ "${CONFIRM}" != "YES" ]; then
    echo "Aborted by user."; exit 1
  fi
fi

# perform deletion
for id in "${ITEMS[@]}"; do
  echo -n "Deleting course '${id}'... "
  ENC=$(python3 -c "import urllib.parse,sys; print(urllib.parse.quote(sys.argv[1], safe=''))" "$id")
  STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "${BASE_URL}/admin/course/${ENC}") || STATUS="000"
  echo "HTTP ${STATUS}"
done

echo "All done."

#!/usr/bin/env bash
# Uninstall all Helm charts installed by `install-all.sh` in the target namespace
# Usage:
#   NAMESPACE=neptun RELEASE_PREFIX=my-release ./helm/scripts/uninstall-all.sh
# To remove the namespace after uninstall, set DELETE_NAMESPACE=true
# Note: make executable with `chmod +x helm/scripts/uninstall-all.sh`

set -euo pipefail

NAMESPACE=${NAMESPACE:-neptun}
RELEASE_PREFIX=${RELEASE_PREFIX:-}
DELETE_NAMESPACE=${DELETE_NAMESPACE:-false}

CHART_DIRS=(
  db-operations-service
  subject-service
  auth-service
  database-service
  caching-service
  gateway
)

# Helper to compute release name
release_name() {
  chart=$1
  if [ -n "$RELEASE_PREFIX" ]; then
    echo "${RELEASE_PREFIX}-${chart}"
  else
    echo "$chart"
  fi
}

echo "Uninstalling releases from namespace: $NAMESPACE"
for chart in "${CHART_DIRS[@]}"; do
  name=$(release_name "$chart")
  echo "-- Uninstalling $name --"
  if helm status "$name" -n "$NAMESPACE" >/dev/null 2>&1; then
    helm uninstall "$name" -n "$NAMESPACE" || true
    echo "Uninstalled $name"
  else
    echo "$name not found in namespace $NAMESPACE, skipping"
  fi
done

if [ "$DELETE_NAMESPACE" = "true" ]; then
  echo "Deleting namespace $NAMESPACE"
  kubectl delete namespace "$NAMESPACE" || true
fi

echo "Done."
exit 0

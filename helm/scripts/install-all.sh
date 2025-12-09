#!/usr/bin/env bash
# Install all Helm charts in the `helm/` folder into namespace (default: neptun)
# Usage:
#   NAMESPACE=neptun RELEASE_PREFIX=my-release ./helm/scripts/install-all.sh
# Note: make executable with `chmod +x helm/scripts/install-all.sh`

set -euo pipefail

NAMESPACE=${NAMESPACE:-neptun}
RELEASE_PREFIX=${RELEASE_PREFIX:-}
# WAIT controls whether `helm upgrade --install` waits for resources to become ready.
# Set to "true" to wait (default: false to avoid hanging on PVCs or long waits).
WAIT=${WAIT:-false}
# Timeout for `--wait` when WAIT is true
TIMEOUT=${TIMEOUT:-2m}

CHART_DIRS=(
  auth-service
  subject-service
  db-operations-service
  caching-service
  database-service
  gateway
)

# Ensure bitnami repo is present (used by some subcharts)
if ! helm repo list | grep -q "bitnami"; then
  echo "Adding bitnami helm repo..."
  helm repo add bitnami https://charts.bitnami.com/bitnami
  helm repo update
fi

# Helper to compute release name
release_name() {
  chart=$1
  if [ -n "$RELEASE_PREFIX" ]; then
    echo "${RELEASE_PREFIX}-${chart}"
  else
    echo "$chart"
  fi
}

echo "Installing charts into namespace: $NAMESPACE"
for chart in "${CHART_DIRS[@]}"; do
  chart_path="./helm/${chart}"
  name=$(release_name "$chart")
  echo "\n-- Installing/upgrading $name from $chart_path --"

  # Update dependencies for the chart (downloads subcharts into charts/)
  (cd "$chart_path" && helm dependency update)

  # Compute extra helm --set args for charts that need in-cluster service URIs
  extra_args=()
  # in-cluster short DNS (service name resolves within same namespace)
  db_release=$(release_name "database-service")
  cache_release=$(release_name "caching-service")
  db_uri="http://${db_release}:80"
  cache_uri="http://${cache_release}:80"

  if [ "$chart" = "auth-service" ]; then
    extra_args+=(--set-string "serviceConfig.databaseServiceUri=${db_uri}" --set-string "serviceConfig.cacheServiceUri=${cache_uri}")
  fi

  if [ "$chart" = "subject-service" ]; then
    extra_args+=(--set-string "servicesConfig.databaseServiceUri=${db_uri}" --set-string "servicesConfig.cachingServiceUri=${cache_uri}")
  fi

  if [ "$chart" = "gateway" ]; then
    auth_release=$(release_name "auth-service")
    subject_release=$(release_name "subject-service")
    auth_uri="http://${auth_release}:80"
    subject_uri="http://${subject_release}:80"
    extra_args+=(--set-string "serviceLocation.authServiceUri=${auth_uri}" --set-string "serviceLocation.subjectServiceUri=${subject_uri}" --set-string "serviceLocation.databaseServiceUri=${db_uri}")
  fi

  helm_cmd=(helm upgrade --install "$name" "$chart_path" --namespace "$NAMESPACE" --create-namespace --dependency-update)

  if [ "$WAIT" = "true" ]; then
    helm_cmd+=(--wait --timeout "$TIMEOUT")
  else
    # Avoid blocking: run without --wait so the script does not hang on unbound PVCs or long pod startups
    echo "NOTE: not waiting for resources to become Ready (set WAIT=true to enable --wait)"
  fi

  "${helm_cmd[@]}" "${extra_args[@]}"

  echo "Deployed $name"
done

echo "\nAll charts deployed."
exit 0

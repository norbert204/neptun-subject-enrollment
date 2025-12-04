#!/usr/bin/env bash
# Install all Helm charts in the `helm/` folder into namespace (default: neptun)
# Usage:
#   NAMESPACE=neptun RELEASE_PREFIX=my-release ./helm/scripts/install-all.sh
# Note: make executable with `chmod +x helm/scripts/install-all.sh`

set -euo pipefail

NAMESPACE=${NAMESPACE:-neptun}
RELEASE_PREFIX=${RELEASE_PREFIX:-}
TIMEOUT=${TIMEOUT:-5m}

CHART_DIRS=(
  caching-service
  database-service
  auth-service
  subject-service
  db-operations-service
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

  helm upgrade --install "$name" "$chart_path" \
    --namespace "$NAMESPACE" --create-namespace \
    --wait --timeout "$TIMEOUT" --dependency-update

  echo "Deployed $name"
done

echo "\nAll charts deployed."
exit 0

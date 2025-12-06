# Helm charts for neptun-subject-enrollment

This directory contains per-service Helm charts for the project. Charts are located in `helm/<service>`.

## Prerequisites

- Docker 18.03 or greater

- Kubectl [install docs](https://kubernetes.io/docs/tasks/tools/install-kubectl-linux/)

- Talosctl [install docs](https://docs.siderolabs.com/talos/v1.11/getting-started/talosctl)

- Helm 3.0 or greater [install docs](https://helm.sh/docs/intro/install/)

- A Kubernetes cluster created with [talosctl local docker setup](https://docs.siderolabs.com/talos/v1.11/platform-specific-installations/local-platforms/docker)

- nginx ingress controller installed in the cluster (e.g. via [Helm chart](https://kubernetes.github.io/ingress-nginx/deploy/#quick-start))

- install rancher local-path provisioner for local PVC provisioning (needed for Cassandra and Redis subcharts) see below for details:

```fish
kubectl apply -f https://raw.githubusercontent.com/rancher/local-path-provisioner/master/deploy/local-path-storage.yaml
```

## Quick deploy (example)

1. Create a namespace:

```fish
kubectl create ns neptun
```

2. Install a chart (example for `subject-service`):

```fish
helm upgrade --install subject-service ./helm/subject-service -n neptun --create-namespace
```
3. Or use scripts found in ./helm/scripts/:

```fish
./helm/scripts/install-all.sh
./helm/scripts/test-services.sh
./helm/scripts/uninstall-all.sh
```


## PVC provisioning (local dev)

If Cassandra or Redis subchart pods stay in `Pending` due to PVCs not being provisioned, you may need to adjust your local provisioner settings to allow hostPath volume creation.

— Allow the local provisioner to create hostPath volumes (quickest for local clusters):

  - Label the provisioner namespace so the provisioner can create hostPath-backed pods (example for the Rancher `local-path` provisioner):

  ```fish
  kubectl label namespace local-path-storage pod-security.kubernetes.io/enforce=privileged --overwrite
  kubectl label namespace local-path-storage pod-security.kubernetes.io/audit=privileged --overwrite
  kubectl label namespace local-path-storage pod-security.kubernetes.io/warn=privileged --overwrite
  ```

  - Recreate or restart the provisioner pods (if needed) and re-deploy the charts:

  ```fish
  kubectl rollout restart deployment -n local-path-storage
  ./helm/scripts/uninstall-all.sh
  ./helm/scripts/install-all.sh
  ```


## Notes for gRPC services behind nginx ingress

The charts set `nginx.ingress.kubernetes.io/backend-protocol: "GRPC"` by default. Ensure your nginx ingress controller supports gRPC and that the service `port` matches the container's gRPC port (the provided Dockerfiles expose port `80`).

If you prefer HTTP routing or need other annotations, set `ingress.annotations` in `values.yaml` to override or extend annotations.

# Helm charts for neptun-subject-enrollment

This directory contains per-service Helm charts for the project. Charts are located in `helm/<service>`.

## Quick deploy (example)

1. Create a namespace:

```fish
kubectl create ns neptun
```

2. If your GHCR images are private, create a Docker registry secret (replace placeholders):

```fish
kubectl create secret docker-registry ghcr-secret \
  --docker-server=ghcr.io \
  --docker-username=<GITHUB_USERNAME> \
  --docker-password=<PERSONAL_ACCESS_TOKEN> \
  --docker-email=you@example.com -n neptun
```

Then set `imagePullSecrets` in the chart `values.yaml` (or pass via `--set imagePullSecrets[0]=ghcr-secret`).

3. Install a chart (example for `subject-service`):

```fish
helm upgrade --install subject-service ./helm/subject-service -n neptun --create-namespace
```

4. Override image tag or ingress host on the CLI:

```fish
helm upgrade --install subject-service ./helm/subject-service -n neptun \
  --set image.tag=1.0.0 \
  --set ingress.enabled=true \
  --set ingress.hosts[0].host=subject.local
```

## Notes for gRPC services behind nginx ingress

The charts set `nginx.ingress.kubernetes.io/backend-protocol: "GRPC"` by default. Ensure your nginx ingress controller supports gRPC and that the service `port` matches the container's gRPC port (the provided Dockerfiles expose port `80`).

If you prefer HTTP routing or need other annotations, set `ingress.annotations` in `values.yaml` to override or extend annotations.

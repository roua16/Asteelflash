#!/usr/bin/env bash
set -euo pipefail

# Starts a SQL Server 2019 container bound to localhost:1433 with SA credentials used by the project.
# Requires Docker.

CONTAINER_NAME=${1:-itstockm-mssql}
SA_PASSWORD=${2:-AsteelFlash@2026}

echo "Pulling SQL Server image..."
docker pull mcr.microsoft.com/mssql/server:2019-latest

if docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
  echo "Container ${CONTAINER_NAME} already exists. Starting it..."
  docker start ${CONTAINER_NAME}
else
  echo "Creating and running container ${CONTAINER_NAME} bound to localhost:1433"
  docker run -e 'ACCEPT_EULA=Y' -e "SA_PASSWORD=${SA_PASSWORD}" \
    -p 1433:1433 --name ${CONTAINER_NAME} -d mcr.microsoft.com/mssql/server:2019-latest
fi

echo "Waiting for SQL Server to be ready..."
# Wait until SQL Server accepts connections
for i in {1..30}; do
  if docker logs ${CONTAINER_NAME} 2>&1 | grep -q 'SQL Server is now ready for client connections'; then
    echo "SQL Server is ready"
    exit 0
  fi
  sleep 2
done

echo "Timed out waiting for SQL Server to be ready. Check container logs: docker logs ${CONTAINER_NAME}"
exit 1

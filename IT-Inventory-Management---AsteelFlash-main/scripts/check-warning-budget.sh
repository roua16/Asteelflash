#!/usr/bin/env bash
set -euo pipefail

BASELINE_FILE=".ci/warning-budget.txt"
BUILD_LOG="${1:-/tmp/itstockm-build.log}"

if [[ ! -f "${BASELINE_FILE}" ]]; then
  echo "Missing baseline file: ${BASELINE_FILE}"
  exit 1
fi

baseline="$(tr -d '[:space:]' < "${BASELINE_FILE}")"

if [[ -z "${baseline}" ]]; then
  echo "Empty warning baseline in ${BASELINE_FILE}"
  exit 1
fi

dotnet build ITStockM.csproj -v minimal > "${BUILD_LOG}" 2>&1

warnings_line="$(grep -E "Build succeeded with [0-9]+ warning\(s\)" "${BUILD_LOG}" | tail -n 1 || true)"
if [[ -n "${warnings_line}" ]]; then
  current="$(echo "${warnings_line}" | sed -E 's/.*with ([0-9]+) warning\(s\).*/\1/')"
else
  # Fallback format:
  # Build succeeded.
  #     0 Warning(s)
  current="$(grep -E "^[[:space:]]*[0-9]+ Warning\(s\)" "${BUILD_LOG}" | tail -n 1 | sed -E 's/^[[:space:]]*([0-9]+) Warning\(s\).*/\1/' || true)"
fi

if [[ -z "${current}" ]]; then
  echo "Could not parse warning count from build output"
  cat "${BUILD_LOG}"
  exit 1
fi

echo "Warning baseline: ${baseline}"
echo "Current warnings: ${current}"

if (( current > baseline )); then
  echo "Warning budget regression detected"
  exit 1
fi

echo "Warning budget check passed"

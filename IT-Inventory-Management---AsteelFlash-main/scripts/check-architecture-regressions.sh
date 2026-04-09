#!/usr/bin/env bash
set -euo pipefail

fail=0

check_empty() {
  local name="$1"
  local cmd="$2"
  local output
  output="$(eval "${cmd}" || true)"
  if [[ -n "${output}" ]]; then
    echo "[FAIL] ${name}"
    echo "${output}"
    fail=1
  else
    echo "[PASS] ${name}"
  fi
}

check_empty "No async void in core paths" "rg -n 'async void' Components Services Controllers Repositories"
check_empty "No fire-and-forget Task.Run in Services" "rg -n '_ = Task\\.Run\\(' Services"
check_empty "No dynamic repository context cast in Services" "rg -n 'as dynamic\\)\\?\\._context|dynamic\\)\\?\\._context|_context\\) as dynamic|as dynamic' Services"
check_empty "No typo-form naming regressions in source" "rg -n 'DeleveryOrder|Descipriton|Descriptoin|Assignmnets|AdddDelayedMaterials' Components Services Controllers Repositories Models Data"

if (( fail != 0 )); then
  exit 1
fi

echo "Architecture regression checks passed"

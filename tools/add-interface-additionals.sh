#!/bin/bash

set -euo pipefail

file="${1}"

tmp_f=$(mktemp $PWD/check_files_XXXXXX)
trap 'rm -f "${tmp_f}"' EXIT

cat "${file}" \
  | (grep -E '(xml|lua)$' || true) \
  > "${tmp_f}"

sed -e "s,.lua$,.xml," "${tmp_f}"
sed -e "s,.xml$,.lua," "${tmp_f}"
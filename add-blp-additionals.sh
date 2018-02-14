#!/bin/bash

set -euo pipefail

file="${1}"

#suffixes="$(echo _{n,e,s,hd,h}.blp)"
suffixes="_n.blp _e.blp _s.blp _hd.blp _h.blp"

tmp_f=$(mktemp $PWD/check_files_XXXXXX)
trap 'rm -f "${tmp_f}"' EXIT

cat "${file}" \
  | (grep blp$ || true) \
  > "${tmp_f}"

sed -e 's,\(.*/\),\1noliquid_,' "${tmp_f}"

for suff in ${suffixes}
do
  sed -e "s,.blp$,${suff}," "${tmp_f}"
done

#!/bin/bash

set -euo pipefail

file="${1}"

#suffixes="$(echo _{n,e,s,hd,h,l,u,m,f}.blp)"
suffixes="_n.blp _e.blp _s.blp _hd.blp _h.blp _l.blp _u.blp _m.blp _f.blp"

tmp_f=$(mktemp $PWD/check_files_XXXXXX)
trap 'rm -f "${tmp_f}"' EXIT

cat "${file}" \
  | (grep blp$ || true) \
  > "${tmp_f}"

grep "world/minimaps/" "${tmp_f}" | sed -e 's,\(.*/\),\1noliquid_,'

for suff in ${suffixes}
do
  sed -e "s,.blp$,${suff}," "${tmp_f}"
  grep '_[a-z].blp$' "${tmp_f}" | sed -e "s,_[a-z].blp$,${suff},"
done

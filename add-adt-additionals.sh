#!/bin/bash

set -euo pipefail

file="${1}"

#suffixes="$(echo _lod.adt _{obj,tex}{0,1}.adt .pm4)"
suffixes="_lod.adt _obj0.adt _obj1.adt _tex0.adt _tex1.adt .pm4"

tmp_f=$(mktemp $PWD/check_files_XXXXXX)
trap 'rm -f "${tmp_f}"' EXIT

cat "${file}" \
  | (grep adt$ || true) \
  > "${tmp_f}"

for suff in ${suffixes}
do
  sed -e "s,.adt$,${suff}," "${tmp_f}"
done

grep "_[0-9]*.adt$" "${tmp_f}" | sed -e 's,world/maps/,world/minimaps/,' -e 's,\(.*[^/]*/\).*_\([0-9]*_[0-9]*\).adt$,\1map\2.blp,'
grep "_[0-9]*.adt$" "${tmp_f}" | sed -e 's,world/maps/,world/maptextures/,' -e 's,\(.*[^/]*/.*\)\([0-9]*_[0-9]*\).adt$,\1\2.blp,'
grep "_[0-9]*.adt$" "${tmp_f}" | sed -e 's,world/maps/,world/maptextures/,' -e 's,\(.*[^/]*/.*\)\([0-9]*_[0-9]*\).adt$,\1\2_n.blp,'

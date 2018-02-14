#!/bin/bash

set -euo pipefail

file="${1}"

suffixes="$(grep '_[0-9][0-9][0-9].wmo$' listfile.txt | sed  -e 's,.*_\([0-9][0-9][0-9]\).wmo,_\1.wmo _\1_lod1.wmo _\1_lod2.wmo _\1_lod1.blp _\1_lod2.blp,' | tr ' ' '\n' | sort -u)"

for suff in ${suffixes}
do
  cat "${file}" \
    | (grep wmo$ || true) \
    | (sed -e 's,_[0-9][0-9][0-9]\(_lod.\)*.wmo$,.wmo,' || true) \
    | sort -u \
    | sed -e "s,.wmo$,${suff},"
done

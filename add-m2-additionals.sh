#!/bin/bash

set -euo pipefail

file="${1}"

suffixes="$(echo .phys .skel 0{0..3}.skin _lod0{1,2}.skin)"
suffixes="$(grep bone$ listfile.txt | sed -e s,.*_,_, | sort -u) ${suffixes}"
suffixes="$(grep anim$ listfile.txt | sed -e 's,.*\([0-9][0-9][0-9][0-9]-[0-9][0-9]\).anim,\1.anim,' | sort -u) ${suffixes}"

for suff in ${suffixes}
do
  cat "${file}" \
    | (grep m2$ || true) \
    | sed -e "s,.m2$,${suff},"
done

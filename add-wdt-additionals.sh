#!/bin/bash

set -euo pipefail

file="${1}"

#suffixes="$(echo _{occ,lgt,fogs,mpv}.wdt .wdl)"
suffixes="_occ.wdt _lgt.wdt _fogs.wdt _mpv.wdt .wdl"

tmp_f=$(mktemp $PWD/check_files_XXXXXX)
trap 'rm -f "${tmp_f}"' EXIT

cat "${file}" \
  | (grep wdt$ || true) \
  > "${tmp_f}"

for suff in ${suffixes}
do
  sed -e "s,.wdt$,${suff}," "${tmp_f}"
done

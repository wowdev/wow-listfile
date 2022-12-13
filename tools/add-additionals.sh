#!/bin/bash

set -euo pipefail
_scriptdir=$($(which greadlink readlink 2>/dev/null | head -n 1) -f ${BASH_SOURCE})
scriptdir="${_scriptdir%/*}"

commitish=${@}

tmp_f=$(mktemp $PWD/check_files_XXXXXX)
trap 'rm -f "${tmp_f}"' EXIT
git show ${commitish} listfile.txt | (grep ^+ || true) | (grep -v ^++ || true) | sed -e s,^+,, > "${tmp_f}"
echo "- trying to find additionals for $(echo $(cat "${tmp_f}" | wc -l)) files..." >&2

for script in "${scriptdir}"/add-*-additionals.sh
do
  "${script}" "${tmp_f}"
done | "${scriptdir}/marlamin-check_files-pipe.sh"

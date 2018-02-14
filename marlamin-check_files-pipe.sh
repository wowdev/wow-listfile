#!/bin/bash
set -uo pipefail
_scriptdir=$($(which greadlink readlink 2>/dev/null | head -n 1) -f ${BASH_SOURCE})
scriptdir="${_scriptdir%/*}"

tmp_f=$(mktemp $PWD/check_files_XXXXXX)

cat > ${tmp_f}

split -a 5 -l 1499 ${tmp_f} ${tmp_f}-part

"${scriptdir}/marlamin-check_files.sh" ${tmp_f}-part*

rm -f ${tmp_f}*

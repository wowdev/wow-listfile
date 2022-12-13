#!/bin/bash
set -uo pipefail
_scriptdir=$($(which greadlink readlink 2>/dev/null | head -n 1) -f ${BASH_SOURCE})
scriptdir="${_scriptdir%/*}"

tmp_f=$(mktemp $PWD/check_files_XXXXXX)
trap 'find ${PWD} -wholename "${tmp_f}*" -delete' EXIT

sort -u > "${tmp_f}"
if ! test -s "${tmp_f}"
then
  echo "- no files to check" >&2
  exit 0
fi
echo "- checking $(head -n1 "${tmp_f}") and $(($(cat "${tmp_f}" | wc -l)-1)) others..." >&2

split -a 5 -l 1000 "${tmp_f}" "${tmp_f}-part"

find ${PWD} -wholename "${tmp_f}-part*" -print0 | xargs -0 "${scriptdir}/marlamin-check_files.sh"

echo "- done" >&2

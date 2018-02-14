#!/bin/bash
set -uo pipefail
_scriptdir=$($(which greadlink readlink 2>/dev/null | head -n 1) -f ${BASH_SOURCE})
scriptdir="${_scriptdir%/*}"

tmp_f=$(mktemp $PWD/check_files_XXXXXX)
trap 'rm -f ${tmp_f}*' EXIT

sort -u > "${tmp_f}"
if ! test -s "${tmp_f}"
then
  echo "- no files to check" >&2
  exit 0
fi
echo "- checking $(head -n1 "${tmp_f}") and $(($(cat "${tmp_f}" | wc -l)-1)) others..." >&2

split -a 5 -l 1000 "${tmp_f}" "${tmp_f}-part"

"${scriptdir}/marlamin-check_files.sh" "${tmp_f}-part"*

echo "- done" >&2

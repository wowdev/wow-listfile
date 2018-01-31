#!/bin/bash
set -uo pipefail

tmp_f=$(mktemp $PWD/check_files_XXXXXX)

cat > ${tmp_f}

split -a 5 -l 1499 ${tmp_f} ${tmp_f}-part

./marlamin-check_files.sh ${tmp_f}-part*

rm -f ${tmp_f}*

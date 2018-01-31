#!/bin/bash
set -uo pipefail

for file in ${@}
do
  wc -l $file
  curl -s https://bnet.marlam.in/checkFiles.php --data files="$(cat ${file})" | sed -e 's,<br>,%,g' | tr '%' '\n' | grep ^Added
done

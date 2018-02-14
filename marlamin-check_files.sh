#!/bin/bash
set -euo pipefail

for file in ${@}
do
  echo "-- checking $(head -n1 "${file}") and $(($(cat "${file}" | wc -l)-1)) others..." >&2
  curl --silent --show-error "https://bnet.marlam.in/checkFiles.php" --data files="$(cat "${file}")" | sed -e 's,<br>,%,g' | tr '%' '\n' | (grep ^Added || true) | sed -e "s,^Added,+,"
done

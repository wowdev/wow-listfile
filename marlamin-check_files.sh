#!/bin/bash
set -euo pipefail

for file in ${@}
do
  echo "-- checking $(head -n1 "${file}") and $(($(cat "${file}" | wc -l)-1)) others..." >&2
  curl --silent --show-error "https://bnet.marlam.in/checkFiles.php" --data-urlencode files="$(cat "${file}")" > ${file}.log

  cat "${file}.log" \
     | sed -e 's,<br>,%,g' \
     | tr '%' '\n' \
     | (grep ^Added || true) \
     | sed -e "s,^Added,+++,"

  cat "${file}.log" \
     | awk '/<h3>Valid files<\/h3><pre>/{flag=1;print;next}/<\/pre>/{flag=0}flag' \
     | sed -e 's,<h3>Valid files</h3><pre>,,' \
     | sed -e 's,^,=== ,'

  rm "${file}.log"
done

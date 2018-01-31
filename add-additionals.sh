#!/bin/bash

set -euo pipefail

commitish=${@}

for script in add-*-additionals.sh
do
  echo "./${script} <(git show ${commitish} | grep ^+ | grep -v ^++ | sed -e s,^+,,)"
done | bash | ./marlamin-check_files-pipe.sh

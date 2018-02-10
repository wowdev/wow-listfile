#!/bin/bash

set -euo pipefail
_scriptdir=$(greadlink -f ${BASH_SOURCE})
scriptdir="${_scriptdir%/*}"

curl "https://bnet.marlam.in/listfile.php" >> listfile.txt
"${scriptdir}/normalize.sh"

git add listfile.txt
git commit -m "bnet.marlam.in listfile $(LANG=C date)"
git push

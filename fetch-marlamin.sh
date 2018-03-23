#!/bin/bash

set -euo pipefail
_scriptdir=$($(which greadlink readlink 2>/dev/null | head -n 1) -f ${BASH_SOURCE})
scriptdir="${_scriptdir%/*}"

curl --compressed "https://bnet.marlam.in/listfile.php" >> listfile.txt
"${scriptdir}/normalize.sh"

if ${1:-false}
then
  git add listfile.txt
  git commit -m "bnet.marlam.in listfile $(LANG=C date)"
  if ${2:-true}
  then
    git push
  fi
fi

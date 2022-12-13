#!/bin/bash

set -euo pipefail
_scriptdir=$($(which greadlink readlink 2>/dev/null | head -n 1) -f ${BASH_SOURCE})
scriptdir="${_scriptdir%/*}"

curl --compressed --silent --show-error "https://bnet.marlam.in/listfile.php" >> listfile.txt
"${scriptdir}/normalize.sh"

if ${1:-false}
then
  git add listfile.txt
  if [ -n "${3-}" ]
  then
    git commit -m "bnet.marlam.in listfile $(LANG=C date)" -m "$3"
  else
    git commit -m "bnet.marlam.in listfile $(LANG=C date)"
  fi
  if ${2:-true}
  then
    git push
  fi
fi

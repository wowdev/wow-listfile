#!/bin/bash

set -euo pipefail
_scriptdir=$($(which greadlink readlink 2>/dev/null | head -n 1) -f ${BASH_SOURCE})
scriptdir="${_scriptdir%/*}"

commitish=${@}

for script in "${scriptdir}"/add-*-additionals.sh
do
  echo "${script} <(git show ${commitish} | grep ^+ | grep -v ^++ | sed -e s,^+,,)"
done | bash | "${scriptdir}/marlamin-check_files-pipe.sh"

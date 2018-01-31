#!/bin/bash

set -euo pipefail

file=${1}

suffixes="$(echo _{occ,lgt,fogs}.wdt .wdl)"

for suff in $suffixes
do
  cat $file | grep wdt$ | sed -e s,.wdt$,$suff,
done

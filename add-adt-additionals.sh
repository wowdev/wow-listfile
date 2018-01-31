#!/bin/bash

set -euo pipefail

file=${1}

suffixes="$(echo _lod.adt _{obj,tex}{0,1}.adt .pm4)"

for suff in $suffixes
do
  cat $file | grep adt$ | sed -e s,.adt$,$suff,
done

#!/bin/bash

set -euo pipefail

file=${1}

suffixes="$(echo _{n,e,s,hd,h}.blp)"

cat $file | sed -e 's,\(.*/\),\1noliquid_,'

for suff in $suffixes
do
  cat $file | grep blp$ | sed -e s,.m2$,$suff,
done

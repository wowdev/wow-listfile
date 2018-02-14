#!/bin/bash

set -euo pipefail

file=${1}

suffixes="$(grep wmo$ listfile.txt | grep '_[0-9][0-9][0-9].wmo' | sed  -e 's,.*_\([0-9][0-9][0-9]\).wmo,\1,'  | sort -u | sed -e s,$,.wmo, -e s,^,_,) $(grep wmo$ listfile.txt | grep '_[0-9][0-9][0-9].wmo' | sed  -e 's,.*_\([0-9][0-9][0-9]\).wmo,\1,'  | sort -u | sed -e s,$,_lod1.wmo, -e s,^,_,) $(grep wmo$ listfile.txt | grep '_[0-9][0-9][0-9].wmo' | sed  -e 's,.*_\([0-9][0-9][0-9]\).wmo,\1,'  | sort -u | sed -e s,$,_lod2.wmo, -e s,^,_,)"

for suff in $suffixes
do
  cat $file | grep wmo$ | sed -e s,.wmo$,$suff,
done

cat $file | grep _lod..wmo$ | sed -e s,.wmo$,.blp,

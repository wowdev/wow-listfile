#!/bin/bash

set -euo pipefail

file=${1}

suffixes="$(echo .phys .skel 0{0..3}.skin _lod0{1,2}.skin) $(grep bone$ listfile.txt | sed -e s,.*_,_, | sort -u) $(grep anim$ listfile.txt | sed -e 's,.*\(0[0-9][0-9][0-9]-[0-9][0-9]\).anim,\1.anim,' | sort -u)"

for suff in $suffixes
do
  cat $file | grep m2$ | sed -e s,.m2$,$suff,
done

#!/bin/bash

set -euo pipefail

tr -s '/\\' '/' < listfile.txt \
    | tr '[:upper:]' '[:lower:]' | tr -d '\015' \
    | sort -u \
    | grep -vi .ds_store \
	   > listfile.tmp

mv listfile.tmp \
   listfile.txt

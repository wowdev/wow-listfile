#!/bin/bash

set -euo pipefail

curl "https://bnet.marlam.in/listfile.php" >> listfile.txt
"./normalize.sh"

git add listfile.txt
git commit -m "bnet.marlam.in listfile $(LANG=C date)"
git push

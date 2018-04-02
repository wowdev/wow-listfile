#!/bin/bash

set -euo pipefail

file="${1}"

sed -e "s,$,.meta," "${file}"

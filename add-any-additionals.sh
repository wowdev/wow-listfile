#!/bin/bash

set -euo pipefail

file="${1}"

(grep -v ".meta$" "${file}" || true) | sed -e "s,$,.meta,"
(grep ".meta$" "${file}" || true) | sed -e 's,.meta$,,'

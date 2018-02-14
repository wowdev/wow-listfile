#!/bin/bash

set -euo pipefail

file="${1}"

#suffixes="$(echo .phys .skel 0{0..3}.skin _lod0{1,2}.skin)"
#suffixes="$(grep bone$ listfile.txt | sed -e s,.*_,_, | sort -u) ${suffixes}"
#suffixes="$(grep anim$ listfile.txt | sed -e 's,.*\([0-9][0-9][0-9][0-9]-[0-9][0-9]\).anim,\1.anim,' | sort -u) ${suffixes}"
suffixes="0004-00.anim 0005-00.anim 0005-01.anim 0013-00.anim 0013-01.anim 0042-00.anim 0042-01.anim 0043-00.anim 0043-01.anim 0044-00.anim 0044-01.anim 0045-00.anim 0060-00.anim 0060-01.anim 0061-00.anim 0061-01.anim 0062-00.anim 0062-01.anim 0062-02.anim 0063-00.anim 0064-00.anim 0065-00.anim 0065-01.anim 0065-02.anim 0066-00.anim 0067-00.anim 0068-00.anim 0069-00.anim 0069-01.anim 0069-02.anim 0069-03.anim 0069-04.anim 0069-05.anim 0070-00.anim 0071-00.anim 0072-00.anim 0073-00.anim 0074-00.anim 0075-00.anim 0076-00.anim 0077-00.anim 0077-01.anim 0078-00.anim 0079-00.anim 0080-00.anim 0081-00.anim 0082-00.anim 0083-00.anim 0084-00.anim 0096-00.anim 0097-00.anim 0097-01.anim 0098-00.anim 0099-00.anim 0100-00.anim 0100-01.anim 0100-02.anim 0101-00.anim 0102-00.anim 0102-01.anim 0103-00.anim 0103-01.anim 0104-00.anim 0104-01.anim 0113-00.anim 0119-00.anim 0120-00.anim 0123-00.anim 0128-00.anim 0129-00.anim 0133-00.anim 0134-00.anim 0134-01.anim 0143-00.anim 0143-01.anim 0185-00.anim 0186-00.anim 0186-01.anim 0195-00.anim 0211-00.anim 0223-00.anim 0223-01.anim 0233-00.anim 0234-00.anim 0234-01.anim 0242-00.anim 0458-00.anim 0463-00.anim 0464-00.anim 0465-00.anim 0506-00.anim 0518-00.anim 0520-00.anim 0522-00.anim 0524-00.anim 0526-00.anim 0534-00.anim 0536-00.anim 0538-00.anim 0540-00.anim 0544-00.anim 0550-00.anim 0551-00.anim 0552-00.anim 0554-00.anim 0556-00.anim 0556-01.anim 0562-00.anim 0590-00.anim 0590-01.anim 0592-00.anim 0596-00.anim 0598-00.anim 0600-00.anim 0604-00.anim 0604-01.anim 0620-00.anim 0622-00.anim 0634-00.anim 0646-00.anim 0732-00.anim 0778-00.anim 0780-00.anim 1168-00.anim 1172-00.anim 1174-00.anim 1176-00.anim 1188-00.anim 1190-00.anim 1192-00.anim 1194-00.anim 1194-01.anim 1194-02.anim 1196-00.anim 1196-01.anim 1198-00.anim 1206-00.anim 1220-00.anim 1274-00.anim 1276-00.anim 1310-00.anim 1312-00.anim 1314-00.anim _00.bone _01.bone _02.bone _03.bone _04.bone _05.bone _06.bone _07.bone _08.bone _09.bone _10.bone _11.bone _12.bone _13.bone _14.bone _15.bone _16.bone _17.bone _18.bone _19.bone _20.bone _21.bone _22.bone _23.bone _24.bone _25.bone _26.bone _27.bone _28.bone _29.bone .phys .skel 00.skin 01.skin 02.skin 03.skin _lod01.skin _lod02.skin"

tmp_f=$(mktemp $PWD/check_files_XXXXXX)
trap 'rm -f "${tmp_f}"' EXIT

cat "${file}" \
  | (grep m2$ || true) \
  > "${tmp_f}"

for suff in ${suffixes}
do
  sed -e "s,.m2$,${suff}," "${tmp_f}"
done

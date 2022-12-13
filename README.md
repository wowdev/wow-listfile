# Listfiles
This repository contains listfiles for use with World of Warcraft as well as some related scripts.

## Community listfile (community-listfile.csv)
As of WoW: Battle for Azeroth, many files have lost their lookup hash which allowed us to verify and map filenames for a file. For that reason, a new listfile was created that binds names to file IDs instead.

### How to add things to community-listfile.csv
Please open a [new issue](https://github.com/wowdev/wow-listfile/issues/new/choose) by picking the "CSV listfile suggestion" and attaching your suggestions in the form of a .txt file. 

## Listfile (listfile.txt)
This listfile should only contain verified and/or official names.

### How to add things to listfile.txt (legacy lookup-based listfile)
* `./normalize.sh`
* if `git status` has modifications, `git add listfile.txt && git commit -m "normalisation permutation"`
* `cat >> listfile.txt`
* `./normalize.sh`
* `git add listfile.txt && git commit -m "$message"`

# Listfiles
This repository contains listfiles for use with World of Warcraft as well as some related scripts.

## Community listfile (community-listfile.csv)
As of WoW: Battle for Azeroth, many files have lost their lookup hash which allowed us to verify and map filenames for a file. For that reason, a new listfile was created that binds names to file IDs instead. This listfile is currently automatically synchronized from wow.tools, where the community can submit filenames to add new files or improve existing names. 

### How to add things to community-listfile.csv
Right now we don't accept PRs/changes to community-listfile.csv. If you want to contribute to this file, make an account on [wow.tools](https://wow.tools), log in and [suggest names](https://wow.tools/files/submitFiles.php). Once the names have been manually verified by a moderator, the changes will be synchronized to GitHub within 5-10 minutes.

## Listfile (listfile.txt)
This listfile should only contain verified and/or official names.

### How to add things to listfile.txt (legacy lookup-based listfile)
* `./normalize.sh`
* if `git status` has modifications, `git add listfile.txt && git commit -m "normalisation permutation"`
* `cat >> listfile.txt`
* `./normalize.sh`
* `git add listfile.txt && git commit -m "$message"`

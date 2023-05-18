# Listfiles
This repository contains listfiles for use with World of Warcraft as well as some related scripts.

## Community listfile
As of WoW: Battle for Azeroth, many files have lost their lookup hash which allowed us to verify and map filenames for a file. For that reason, a new listfile was created that binds names to file IDs instead. There are 2 different listfiles available, `community-listfile.csv` which is normalized to only contain lowercase filenames and `community-listfile-withcapitals.csv` with varied casing for user friendliness.

### How to add things
Please open a [new issue](https://github.com/wowdev/wow-listfile/issues/new/choose) by picking the "CSV listfile suggestion" and attaching your suggestions in the form of a .txt file. If you are instead submitting a pull request on the listfiles directly, make sure that `community-listfile-withcapitals.csv` is the leading listfile preferable with proper casing, and `community-listfile.csv` being exactly the same, just lowercase. Changes made only to `community-listfile.csv` might be lost. 

### Note on filename "stability"
Due to lookup hashes largely being unavailable we are no longer able to verify the filenames for most files. As such, all files with unverifiable filenames (most files since 8.2) can change their name at any time if a better name is thought of/submitted, especially if the current filename is a placeholder filename (for example by having `unk`, `unknown`, `autogen` or a filedataid in the name). If you work on/with tools that rely on the filenames in the listfile being "stable", please keep this note in mind. 

## Listfile (listfile.txt)
This listfile should only contain verified and/or official names and as such will not have many names from modern WoW expansions.

### How to add things to listfile.txt (legacy lookup-based listfile)
* `./normalize.sh`
* if `git status` has modifications, `git add listfile.txt && git commit -m "normalisation permutation"`
* `cat >> listfile.txt`
* `./normalize.sh`
* `git add listfile.txt && git commit -m "$message"`

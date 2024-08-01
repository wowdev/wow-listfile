# ⚠Important update⚠ 
_(Last updated August 1st 2024 @ 02:15 CEST)_  

**_TL;DR Use the listfiles from releases from now on, NOT the listfiles in the repository as these have been removed._**

Direct links for the latest full listfile from the latest release:  
**[community-listfile.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/community-listfile.csv)**  
**[community-listfile-withcapitals.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/community-listfile-withcapitals.csv)**

Due to the 100MB size limit on files inside GitHub repos, complete versions of the community listfiles will from now on be distributed [through GitHub releases](https://github.com/wowdev/wow-listfile/releases).

**Update May 23rd 2024:** As announced in August 2023, the community listfiles in the repository will soon be removed.

**Update June 27th 2024:** The listfiles in the repo are no longer updated because they hit max size again. Only listfiles from releases will still receive updates.

**Update August 1st 2024:** The in-repo listfiles have now been removed as announced in August 2023.

----

# Listfiles
This repository contains filenames and tools to generate listfiles for use with World of Warcraft.

## Downloading
When downloading one of the listfiles, make sure to use the one from the latest release as the ones in the repository itself are either outdated or missing certain files to stay under the 100MB file limit imposed by GitHub.  

Direct links to the latest released listfiles:  
- **[community-listfile.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/community-listfile.csv)**  
- **[community-listfile-withcapitals.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/community-listfile-withcapitals.csv)**
- **[verified-listfile.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/verified-listfile.csv)**
- **[verified-listfile-withcapitals.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/verified-listfile-withcapitals.csv)**

## Contributing
Please open a [new issue](https://github.com/wowdev/wow-listfile/issues/new/choose) by picking the "CSV listfile suggestion" and attaching your suggestions in the form of a .txt file. Do not make pull requests. When accepted, files will be be merged into the community listfile, and any official/verified names merged into the verified listfile.

## Community listfile
As of WoW: Battle for Azeroth, many files have lost their lookup hash which allowed us to verify and map filenames for a file. For that reason, a new listfile was created that binds names to file IDs instead. There are 2 different listfiles available, `community-listfile.csv` which is normalized to only contain lowercase filenames and `community-listfile-withcapitals.csv` with varied casing for user friendliness.

### Note on filename "stability" in the community listfile
Due to lookup hashes largely being unavailable we are no longer able to verify the filenames for most files. As such, all files with unverifiable filenames (most files since 8.2) can change their name at any time if a better name is thought of/submitted, especially if the current filename is a placeholder filename (for example by having `unk`, `unknown`, `autogen` or a FileDataID in the name). If you work on/with tools that rely on the filenames in the listfile being "stable", please keep this note in mind or use the verified listfile below.

## Verified listfile
Similar to the below listfile, but is formatted like the community listfile and is automatically updated with new verified/official names based on lookups in the meta/lookups.csv file (that one is manually updated still, for now). Also has two versions like the community listfile, one with capitals and one without.

## Listfile (listfile.txt)
This listfile should only contain verified and/or official names from throughout WoW's history and as such will not have many names from modern WoW expansions. This is not automatically updated nor is it available in releases.

### How to add things to listfile.txt (legacy lookup-based listfile)
* `./normalize.sh`
* if `git status` has modifications, `git add listfile.txt && git commit -m "normalisation permutation"`
* `cat >> listfile.txt`
* `./normalize.sh`
* `git add listfile.txt && git commit -m "$message"`

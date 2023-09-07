# ⚠Important update⚠ 
_(Last updated September 8th 2023 @ 01:30 CEST)_  

**_TL;DR Use the listfiles from releases from now on, NOT the listfiles in the repository._**

Direct links for the latest full listfile from the latest release:  
**[community-listfile.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/community-listfile.csv)**  
**[community-listfile-withcapitals.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/community-listfile-withcapitals.csv)**

**Longer explanation:** Due to the 100MB size limit on files inside Git repositories, complete versions of both `community-listfile.csv` and `community-listfile-withcapitals.csv` will from now on be distributed [through GitHub releases](https://github.com/wowdev/wow-listfile/releases). The compiled listfiles currently in the repository do not have filenames that end with `.dat`, `.unk` or `.meta` to save on space to temporarily allow them to stay updated with new files/changes. Further space saving will be applied by removing placeholder/unknown filenames until that no longer works, at which point the latest updated listfile will only be available through releases. The listfiles in the repo will stay for a while for backwards compatibility, but will likely be removed at some point in the future once they haven't been updated for a while.

**Update September 8th 2023**
Due to the large amount of files added in 10.2, all files from parts/placeholder.csv will now also be kept out of the in-repository listfile. This removes all filenames that contain their own FileDataID as well as other placeholder names. The list of rules that dictate a file being a placeholder file is available [here](https://github.com/wowdev/wow-listfile/blob/master/tools/ListfileTool/Program.cs#L346-L351).
If you want a listfile with these files included, I strongly urge you to switch to the release version of the listfile if you haven't already.

----

# Listfiles
This repository contains listfiles for use with World of Warcraft as well as some related scripts/tools.

## Community listfile
As of WoW: Battle for Azeroth, many files have lost their lookup hash which allowed us to verify and map filenames for a file. For that reason, a new listfile was created that binds names to file IDs instead. There are 2 different listfiles available, `community-listfile.csv` which is normalized to only contain lowercase filenames and `community-listfile-withcapitals.csv` with varied casing for user friendliness.

### Downloading
When downloading one of the community listfiles, make sure to use the one from the latest release as the ones in the repository itself are either outdated or missing certain files to stay under the 100MB file limit imposed by GitHub.  

Direct links to the latest released listfile:  
- **[community-listfile.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/community-listfile.csv)**  
- **[community-listfile-withcapitals.csv](https://github.com/wowdev/wow-listfile/releases/latest/download/community-listfile-withcapitals.csv)**

### How to add things
Please open a [new issue](https://github.com/wowdev/wow-listfile/issues/new/choose) by picking the "CSV listfile suggestion" and attaching your suggestions in the form of a .txt file. Do not make pull requests.

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

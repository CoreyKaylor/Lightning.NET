#!/bin/bash
rm -rf artifacts
if ! type dnvm > /dev/null 2>&1; then
    curl -sSL https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.sh | DNX_BRANCH=dev sh && source ~/.dnx/dnvm/dnvm.sh
fi

type make >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'make' for lmdb native lib compile.  Aborting."; exit 1; }
type gcc >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'gcc' for lmdb native lib compile.  Aborting."; exit 1; }

cd mdb/libraries/liblmdb/
make
cd ../../../

# HACK - dnu restore with beta4 fails most of the time
# due to timeouts or other failures.
# Fetch the latest dnu and use that instead
dnvm update-self
dnvm upgrade -unstable
dnu restore
# end hack

dnvm install 1.0.0-beta4
dnvm alias default 1.0.0-beta4
dnu restore
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi
LD_LIBRARY_PATH=./mdb/libraries/liblmdb/:$LD_LIBRARY_PATH dnx ./tests/LightningDB.Tests test
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

#!/bin/bash
rm -rf artifacts
if ! type dotnet > /dev/null 2>&1; then
    curl -sSL https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.sh | bash /dev/stdin --version 1.0.0-preview1-002702 --install-dir ~/dotnet
    sudo ln -s ~/dotnet/dotnet /usr/local/bin
fi

type make >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'make' for lmdb native lib compile.  Aborting."; exit 1; }
type gcc >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'gcc' for lmdb native lib compile.  Aborting."; exit 1; }

cd mdb/libraries/liblmdb/
make
cd ../../../src/LightningDB.Tests

dotnet restore
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi
LD_LIBRARY_PATH=../../mdb/libraries/liblmdb/:$LD_LIBRARY_PATH dotnet test
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

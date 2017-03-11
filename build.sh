#!/bin/bash
type make >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'make' for lmdb native lib compile.  Aborting."; exit 1; }
type gcc >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'gcc' for lmdb native lib compile.  Aborting."; exit 1; }

cd mdb/libraries/liblmdb/
make
cd ../../../
dotnet restore src/Lightning.Net.sln
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi
cd src/LightningDB.Tests
NATIVE_DLL_SEARCH_DIRECTORIES=../../mdb/libraries/liblmdb/:$NATIVE_DLL_SEARCH_DIRECTORIES dotnet test -f netcoreapp1.1
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

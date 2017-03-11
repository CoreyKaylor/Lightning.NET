#!/bin/bash
type make >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'make' for lmdb native lib compile.  Aborting."; exit 1; }
type gcc >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'gcc' for lmdb native lib compile.  Aborting."; exit 1; }

cd mdb/libraries/liblmdb/
make
cd ../../../
dotnet restore src/Lightning.Net.sln
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi
cd src/LightningDB.Tests
dotnet build -f netcoreapp1.1
mv ../../mdb/libraries/liblmdb/liblmdb.so bin/Debug/netcoreapp1.1/liblmdb.so
ls bin/Debug/netcoreapp1.1/
dotnet test -f netcoreapp1.1
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

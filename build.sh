#!/bin/bash
type make >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'make' for lmdb native lib compile.  Aborting."; exit 1; }
type gcc >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'gcc' for lmdb native lib compile.  Aborting."; exit 1; }

cd mdb/libraries/liblmdb/
make
if [[ "$OSTYPE" == "darwin"* ]]; then
    gcc -dynamiclib -o lmdb.dylib mdb.o midl.o
fi
cd ../../../
dotnet restore src/Lightning.Net.sln
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi
cd src/LightningDB.Tests
dotnet build -f netcoreapp2.0
if [[ "$OSTYPE" == "darwin"* ]]; then
    mv ../../mdb/libraries/liblmdb/lmdb.dylib bin/Debug/netcoreapp2.0/lmdb.dylib
else
    mv ../../mdb/libraries/liblmdb/liblmdb.so bin/Debug/netcoreapp2.0/liblmdb.so
fi
ls bin/Debug/netcoreapp2.0/
dotnet test -f netcoreapp2.0
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

#!/bin/bash
rm -rf artifacts
if ! type dotnet > /dev/null 2>&1; then
    sudo sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet/ trusty main" > /etc/apt/sources.list.d/dotnetdev.list'
    sudo apt-key adv --keyserver apt-mo.trafficmanager.net --recv-keys 417A0893
    sudo apt-get update
    sudo apt-get -y install dotnet-dev-1.0.0-preview1-002702
fi

type make >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'make' for lmdb native lib compile.  Aborting."; exit 1; }
type gcc >/dev/null 2>&1 || { echo >&2 "Can't find dependency 'gcc' for lmdb native lib compile.  Aborting."; exit 1; }

cd mdb/libraries/liblmdb/
make
cd ../../../
cd workaround/LibLmdb
dotnet restore
dotnet pack --configuration Release --output ../../artifacts
cd ../../src/LightningDB.Tests

dotnet restore
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi
LD_LIBRARY_PATH=../../mdb/libraries/liblmdb/:$LD_LIBRARY_PATH dotnet test
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

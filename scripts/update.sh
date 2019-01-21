#! /bin/bash

if [ ! -d cycling-backend ]
then
    git clone git@github.com:anyways-open/cycling-backend.git
fi

cd cycling-backend
git pull
cd ..

if [ ! -d idp ]
then
    git clone git@github.com:itinero/idp.git
fi

cd idp
git pull
./build.sh
./publish.sh
cd ..

rm *.sh
cp cycling-backend/scripts/* .
chmod u+x *.sh

./update-map.sh


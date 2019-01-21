# To be run on a dev machine. Creates a test database

if [ ! -f belgium-latest.osm.pbf ]
then
    echo "Downloading the latest map from itinero"
    wget http://files.itinero.tech/data/OSM/planet/europe/belgium-latest.osm.pbf
fi


echo "Downloading the latest profiles"
if [ ! -d routing-profiles ]
then
    git clone git@github.com:anyways-open/routing-profiles.git
fi


cd routing-profiles
git pull
cd ..

if [ -f routing-profiles/bicycle.lua ]
then
#    rm bicycle.lua
#    cp routing-profiles/bicycle.lua .
    echo "Using the old bicycle profile"
fi


echo "Running IDP"
IDP="./idp/src/IDP/bin/release/netcoreapp2.1/linux-x64/IDP"
$IDP --read-pbf belgium-latest.osm.pbf --pr --create-routerdb vehicles=bicycle.lua --write-routerdb staged.belgium.routerdb

# The routerdb is first written to staged.belgium.routerdb
# This is to prevent that the code sees a half-finished DB
# Once done, we rename it

mv staged.belgium.routerdb ../src/mapdata/belgium.routerdb

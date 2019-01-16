# To be run on the server

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
    rm bicycle.lua
    cp routing-profiles/bicycle.lua .
fi


echo "Downloading the latest map from itinero"
rm belgium-latest.osm.pbf
wget http://files.itinero.tech/data/OSM/planet/europe/belgium-latest.osm.pbf



echo "Running IDP"
IDP="./idp/src/IDP/bin/release/netcoreapp2.1/linux-x64/IDP"
$IDP --read-pbf belgium-latest.osm.pbf --pr --create-routerdb vehicles=bicycle.lua --elevation --contract "bicycle" --contract "bicycle.shortest" --contract "bicycle.balanced" --contract "bicycle.networks" --contract "bicycle.genk" --contract "bicycle.relaxed" --write-routerdb staged.belgium.routerdb

# The routerdb is first written to staged.belgium.routerdb
# This is to prevent that the code sees a half-finished DB
# Once done, we rename it

rm belgium.routerdb
mv staged.belgium.routerdb belgium.routerdb

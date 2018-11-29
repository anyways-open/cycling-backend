# To be run on the server

echo "Downloading the latest map from geofabrik"
rm belgium-latest.osm.pbf
wget http://files.itinero.tech/data/OSM/planet/europe/belgium-latest.osm.pbf

echo "Downloading the latest profiles"
rm bicycle.lua
wget -O bicycle.lua https://raw.githubusercontent.com/anyways-open/routing-profiles/master/bicycle.lua?token=ABZgbuydBw_2nzVgLCaFU39i40OlEa8-ks5cBo4twA%3D%3D

echo "Running IDP"
IDP="./idp/src/IDP/bin/release/netcoreapp2.1/linux-x64/IDP"
$IDP --read-pbf belgium-latest.osm.pbf --pr --create-routerdb vehicles=bicycle.lua --elevation --contract "bicycle" --contract "bicycle.shortest" --contract "bicycle.balanced" --contract "bicycle.networks" --contract "bicycle.brussels" --contract "bicycle.relaxed" --write-routerdb staged.belgium.routerdb

# The routerdb is first written to staged.belgium.routerdb
# This is to prevent that the code sees a half-finished DB
# Once done, we rename it

rm belgium.routerdb
mv staged.belgium.routerdb belgium.routerdb

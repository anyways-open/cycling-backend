# To be run on the server

cd /var/data/mapdata
echo "Downloading the latest map from geofabrik"
rm belgium-latest.osm.pbf
wget https://download.geofabrik.de/europe/belgium-latest.osm.pbf

echo "Downloading the latest profiles"
rm bicycle.lua
wget https://raw.githubusercontent.com/anyways-open/cycling-backend/master/src/profiles/bicycle.lua

echo "Running IDP"
IDP="/var/data/mapdata/idp/src/IDP/bin/release/netcoreapp2.1/linux-x64/IDP"
$IDP --read-pbf /var/data/mapdata/belgium-latest.osm.pbf --pr --create-routerdb vehicles=/var/data/mapdata/bicycle.lua --write-routerdb belgium.routerdb



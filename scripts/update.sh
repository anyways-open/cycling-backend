rm *.sh
wget https://raw.githubusercontent.com/anyways-open/cycling-backend/master/scripts/update.sh
wget https://raw.githubusercontent.com/anyways-open/cycling-backend/master/scripts/update-map.sh
wget https://raw.githubusercontent.com/anyways-open/cycling-backend/master/scripts/create-docker.sh
wget https://raw.githubusercontent.com/anyways-open/cycling-backend/master/scripts/update-docker.sh
wget https://raw.githubusercontent.com/anyways-open/cycling-backend/master/scripts/run-docker.sh


chmod u+x *.sh


./update-map.sh


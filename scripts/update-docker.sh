#! /bin/bash

# This script pulls from docker and deploys on changes


# loads and executes the docker image
# to be run on the server


STATE=`docker pull anywaysopen/cycling-backend`
echo $STATE | grep "newer image"
if [[ $STATE =~ "newer image" ]]
then
	date
	echo "Got a newer update. Deploying now..."
	./run-docker.sh
else
	echo "Dockercontainter hasn't been updated, no need to redeploy"	
fi
echo $STATE

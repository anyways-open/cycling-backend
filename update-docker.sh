#! /bin/bash

# This script pulls from docker and deploys on changes


STATE=`docker pull anywaysopen/cycling-backend`
if [[ $STATE == *"Status: Downloaded newer image for anywaysopen/cycling-backend:latest"* ]]
then

	echo "Got a newer update. Deploying now..."
	

fi

# loads and executes the docker image
# to be run on the server

# docker load -i cycling-backend.tar

STATE=`docker pull anywaysopen/cycling-backend
echo $STATE`
if [[ $STATE = *"newer image"* ]]
then

	echo "Got a newer update. Deploying now..."
	./run-docker.sh
else
	echo "Dockercontainter hasn't been updated, no need to redeploy"	

fi

# to be run from the directory it is found in
# will create a docker image. When 'deploy' is specified as first argument, will create a tar and send it over to the server

cd ../src
echo "Starting docker build..."
docker build -t cycling-backend . 


if [ $1 == "run" ]
then
    echo "Starting docker"
    docker rm cycling-backend
    docker run -it --name cycling-backend -v /home/pietervdvn/git/cycling-backend/src/mapdata:/var/app/data -p 5000:5000 cycling-backend
fi

if [ $1 == "deploy" ]
then
    echo "Creating docker file"
    docker save cycling-backend > cycling-backend.tar
    echo "Deploying to server"
    scp cycling-backend.tar.zip root@cycling-backend.anyways.eu:/var/local
    scp run-docker.sh root@cycling-backend.anyways.eu:/var/local
    rm cycling-backend.tar
fi



cd src
docker build -t cycling-backend . 
docker rm rideaway-backend
if [ $1 == "run" ]
then
    echo "Starting docker"
    docker run -it --name rideaway-backend -v /home/pietervdvn/git/cycling-backend/src/mapdata:/var/app/data -p 5000:5000 cycling-backend
fi

if [ $1 == "deploy" ]
then
    echo "Deploying to server"
    docker save cycling-backend > cycling-backend.tar
    scp cycling-backend.tar root@cycling-backend.anyways.eu:/var/local
fi



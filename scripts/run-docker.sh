# loads and executes the docker image
# to be run on the server

docker load -i cycling-backend.tar
docker stop cycling-backend
docker rm cycling-backend
docker run -d --name cycling-backend -v /var/data/mapdata:/var/app/data -v /var/app/logs:/var/app/logs -p 5000:5000 cycling-backend
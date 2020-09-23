const net = require('net');

//Stores all of the connected sockets and their ids
var sockets = [];

//Stores all of the socket ids currently in use
var ids = [];

const server = net.createServer(function(_socket){
    //Print 'client connected' when a client connects
    console.log('client connected: ' + _socket.address().port);

    //These next lines configure event handlers for the socket

    //Configure the 'end' even handler
    //Runs when a client disconnects
    _socket.on('end', () => {
        removeDeadSockets();
    });

    //Configure the 'data' event handler
    //Runs when the socket receives data
    _socket.on('data', function(buf){
        //Parse the message to determine which socket sent it and respont accordingly
        parseMessage(buf);
    });

    //Create a random id for this socket
    getRandomSocketId(1000, function(rand){
        console.log('rand callback')

        //Send the client the random id we made for it
        _socket.write(`${rand}`);

        //Store the socket and the random id as a pair
        console.log(rand);
        sockets.push([_socket,rand]);
    });
});

//Parse a message to determine which socket sent it
function parseMessage(message) {
    //Split the message into the socket's id and the actual message
    var message_split = message.toString().split(',');

    //Print the information to the console
    console.log(message_split[1] + ' sent by ' + message_split[0]);
}

//Called if there is an error
server.on('error', (err) => {
    throw err;
});

//Creates the server that listens for new client connections
server.listen(8124, () => {
    console.log('server bound');
});

//Gets a random id for a new socket that is not already in use
function getRandomSocketId(max, callback) {

    var valid = false;
    var rand = 0;

    //Loop until we find a vaild id
    while(!valid) {
        //Generate a new random id
        rand = Math.floor(Math.random() * Math.floor(max));
        
        //Assume the new id is valid 
        valid = true;

        //Check to see if the new id already exists
        if(ids.includes(rand)){
            valid = false;
        }
        else
        {
            //Add this id to the list of ids that are in use
            ids.push(rand);

            //Return the new id
            return callback(rand);
        }
    }
}

//Removes dead sockets from the socket array and frees their socketIds
function removeDeadSockets() {
    //Array to store the remaining sockets
    var newSockets = [];

    //Loop through all sockets to find the dead ones
    sockets.forEach(function(socket,idx) {
        
        //Check if the socket is dead
        if(socket[0].destroyed){
            //If the socket is dead log it to the console
            console.log('client ' + socket[1] + ' disconnected');

            //And release its socketId so that it can be reused
            releaseSocketId(socket[1]);
        }
        else {
            //If it is still alive add it to the list of remaining sockets
            newSockets.push(socket);
        }

        //If we are on the last iteration of the for loop
        if(idx == sockets.length - 1){
            //Replace the sockets array with the array containing the remaining sockets
            sockets = newSockets;
        }
    });
}

//Removes an id from the ids array
function releaseSocketId(idIn) {
    //Array to store the remaing ids
    var newIds = [];

    //Loop through all ids
    ids.forEach(function(id,idx) {
        //If they are not the one we are trying to remove add them to the new array
        if(id != idIn) {
            newIds.push(id);
        }

        //If we are on the last iteration of the for loop
        if(idx == ids.length - 1) {
            //Replace the ids array with the array containing the remaining ids
            ids = newIds;
        }
    });
}
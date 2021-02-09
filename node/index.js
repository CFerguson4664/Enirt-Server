const net = require('net');

const numIndObj = 100;
const orbsPerSecond = 20;
const orbsFullSyncDelay = 20000; //ms

//Stores all of the connected sockets and their ids
var sockets = [];

//Stores all of the socket ids currently in use
var ids = [];

const server = net.createServer(function(_socket){
    //Print 'client connected' when a client connects
    console.log('client connected: ' + _socket.address().port);

    //These next lines configure event handlers for the socket

    //Configure the 'end' event handler
    //Runs when a client disconnects
    _socket.on('end', function(){
        removeDeadSockets();
    });

    //Configure the 'data' event handler
    //Runs when the socket receives data
    _socket.on('data', function(buf){
        //Parse the message to determine which socket sent it and respond accordingly
        parseMessage(buf);
    });

    //Create a random id for this socket
    getRandomSocketId(1000, function(rand){
        console.log('rand callback')

        //Send the client the random id we made for it
        _socket.write(`${rand}`);

        //Store the socket and the random id as a pair
        console.log(rand);
        sockets.push([_socket,rand,false]);
    });
});

//Parse a message to determine which socket sent it
function parseMessage(message) {

    var messages = message.toString().split("$");

    // console.log('Message is ' + message);
    // console.log('Messages is ' + messages);

    messages.forEach(function(singleMessage) {
        if(singleMessage != '') {
            // console.log('Running messages foreach')
            // console.log('Single message is ' + singleMessage);

            //Split the message into the socket's id and the actual message
            var message_split = singleMessage.toString().split(',');

            // console.log(message_split);

            //Split the message into its parts
            var socketId = message_split[0];
            var command = message_split[1];
            var data = message_split[2];

            //Log information about the message to the console for debugging
            
            // console.log('Socket Id: ' + socketId);
            // console.log('Command  : ' + command);
            // console.log('Data     : ' + data);
            // console.log('');

            //Execute the correct function based on the command
            if(command == 0) {
                //Print the information to the console
                // console.log('Command: Do nothing');
            }
            else if(command == 1) {
                //Exectue the clock sync function
                // console.log('Command: Clock Sync');
                clockSync(socketId);
            }
            else if(command == 2) {
                //Relay message to all clients other than sender
                // console.log('Command: Relay to all other clients');
                relayOthers(data,socketId);
            }
            else if(command == 3) {
                //Log that this socket is redy to recieve player data
                // console.log('Command: Socket Ready');
                readySocket(socketId);
            }
            else if(command == 50) {
                //Relay message to all clients including sender
                // console.log('Command: Relay to all clients');
                relayAll(data);
            }
        }
    });
}

//Sends a message to the client with the given socketId
function sendMessage(message, socketId) {
    //TO DO
    //Function to send a message to the client

}

//Called if there is an error
server.on('error', function(err){
    throw err;
});

//Creates the server that listens for new client connections
server.listen(8124, function(){
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

function readySocket(socketId) {
    console.log('client ' + socketId + ' ready');
    //Iterate through the list of sockets until we find the one that sent the message
    sockets.forEach(function(socket) {
        //Once we find it
        if (socket[1] == socketId) {
            //Set the socket to ready
            socket[3] = true;
            //Request a client to send the current list of orbs
            requestFullOrbSync();

            if(sockets.length == 1) {
                requestGenIndObj();
            } 
            else {
                requestFullIndObjSync();
            }
        }
    })
}

//Called when a client sends the clockSync command
function clockSync(socketId){
    //Iterate through the list of sockets until we find the one that sent the message
    sockets.forEach(function(socket) {
        //Once we find it
        if (socket[1] == socketId) {
            //Reply with the current time
            socket[0].write(Date.now().toString() + "$")
        }
    })
}

//Relay the message to all connected clients 
function relayAll(message) {
    sockets.forEach(function(socket) {
        if(!socket[0].destroyed && socket[3]) {
            socket[0].write(message.toString() + "$");
        }
    });
}

//Relay the message to all clients other than sender id
function relayOthers(message, senderId) {
    sockets.forEach(function(socket) {
        if(!socket[0].destroyed  && socket[3]) {
            if(socket[1] != senderId) {
                socket[0].write(message.toString() + "$");
            }
        }
    });
}

function requestFullOrbSync() {
    console.log('Requesting Orb Sync');

    var syncMessage = '2|sync';
    if(sockets.length > 0){
        sockets[0][0].write(syncMessage + "$");
    }
}

function requestFullIndObjSync() {
    console.log('Requesting IndObj Sync');

    var syncMessage = '5|sync';
    if(sockets.length > 0) {
        sockets[0][0].write(syncMessage + "$");
    }
}

function requestGenIndObj() {
    console.log('Requesting Gen IndObj');

    var syncMessage = '4|' + numIndObj;

    if(sockets.length > 0) {
        sockets[0][0].write(syncMessage + "$");
    }
}

setInterval(function() { 
    if(sockets.length > 0) {
        removeDeadSockets(); 
    }
}, 1000);

setInterval(function() {
    if(sockets.length > 0) {
        removeDeadSockets();

        var time  = Date.now();

        var randMessage = '1|'
        randMessage +=  Math.random().toFixed(2);
        randMessage += ":" + Math.random().toFixed(2);
        randMessage += ":" + time.toString();

        //Send two random numbers for each orb
        for (var i = 1; i < orbsPerSecond; i++) {
            randMessage += "?" + Math.random().toFixed(4);
            randMessage += ":" + Math.random().toFixed(4);
            randMessage += ":" + (time + (1000 / orbsPerSecond) * i).toFixed(0);
        }

        //Send the numbers to all of the sockets
        sockets.forEach(function(socket) {
            console.log('Sending orb loop')
            if(!socket[0].destroyed && socket[3]) {
                socket[0].write(randMessage + "$");
            }
        });
    }
    
}, 1000);

setInterval(function() { 
    if(sockets.length > 0) {
        removeDeadSockets();
        requestFullOrbSync(); 
    }
}, orbsFullSyncDelay);
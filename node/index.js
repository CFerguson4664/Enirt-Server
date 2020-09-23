var express = require('express');

var app = express();

var http = require('http').Server(app);

var sIO = require('socket.io')(http); //Put this with your other variables

//Add this to the end of the document
sIO.on('connection', function(socket) {
    console.log('Someone connected, yippee!');
    sIO.emit('chat message', 'Hello!!!'); //Display the incoming message
});

http.listen(3000, function() {
    console.log('We are listening on port 3000'); //Log a successful connection in the terminal
});
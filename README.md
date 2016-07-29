Signalr Server and Mindwave Mobile data reader for the [mindbot](https://github.com/rdlucas2/mindbot).

WPF app used to start the signalr server which broadcasts messages to the node app listening on the raspberry pi attached to the mindbot.

Pair the mindwave mobile to your device, then start this application. Start the signalr server, connect the signalr client, then connect the headset (must be paired and on). Now, the headset should broadcast through the signalr server to the mindbot. If the mindbot is setup properly, just give it power and it will automatically connect to the signalr server and begin receiving commands.

You can adjust the bot "mode" - or control scheme from this app. Also includes some instructions on the setup. 
from RTNetClient import RTNetClient, RTConnectionStatus

client = RTNetClient("127.0.0.1", 4434)
while(client.connection_status != RTConnectionStatus.Disconnected):
	print "While loop"
print "Ending test..."
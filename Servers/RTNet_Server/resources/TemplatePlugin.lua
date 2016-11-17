 -- Template Lua Plugin

function onEnabled()
	logDebug("Test Lua plugin enabled!")
end

function onDisabled()
	log("Test lua plugin was disabled")
end

function clientConnected(id)
	log("Got new client with id " .. tostring(id))
end

function clientDisconnect(id)
	log("(" .. tostring(id) .. ") disconnected")
end
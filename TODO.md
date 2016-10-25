# TODO List - *RTNet*

#### In General
 - Create some development videos
 - Advertise a little, get word around
 - Create and test on multiple platforms
    * *Windows* **works as executable**
    * *Ubuntu* **works from terminal**
    * *Mac* **works from terminal**

#### Server
 - Fix bug that keeps displaying *"Could not send data"* but data seems to be sent anyway
 - Allocation of IDs is not perfect, sometimes doesn't remove an ID when needed
 - Need to implement end-to-end encryption
 - Need to authenticate the client to make sure they are *legit*
    * Check against Jake's database for account type and authenticity of account
 - Need to start making a master server
 - Fix error with sending packets back to client when *size to send* > *buffer size*
 - Create a proper *Mac OSX* .app - not just a terminal application
 - Log everything to 'logs/log.txt' and all errors to 'logs/errors.txt'
 
#### Client
 - Templates
# RennTek-Networking

## RennTek Networking
The RennTek Network is a cross platform network API meant to be an easy-to-use solution for all developers.

## Currently supported client languages:
 - C#

## How to use RennTek Networking
### The server...
All RennTek Networking servers are hosted by RennTek themselves,
   if you want to host yourself contact us and provide enough
   details about why you can't use our services
   
### The client...
#### C&#35;
The client namespace is entirely within RTNet, which means you'll
   have to be `using RTNet;` before accessing any of the client functions
   
In the simplest form, a client could be made of
```cs
using RTNet;

static void Main(string[] args)
{
   RTClient client = new RTClient("127.0.0.1", 4434);
   while(client.isConnected)
      System.Threading.Thread.Sleep(10); // let the console sleep, the client is in another thread
   // Get rid of any resources the client is using
   client.Dispose();
}
```


####RennTek Studios™ (2014-2016)


###Installation of server###
Just Run the "Automatically Install Binarys.bat"

It will do all the work for you
if you don't have winrar use 7s
winrar "http://rarlab.com/"

###HOW TO USE GIT REMOTE###
or create a new repository on the command line

echo "# RTNET" >> README.md
git init
git add README.md
git commit -m "first commit"
git remote add origin https://github.com/renntekstudios/RTNET.git
git push -u origin master

…or push an existing repository from the command line

git remote add origin https://github.com/renntekstudios/RTNET.git
git push -u origin master
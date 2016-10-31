# RennTek-Networking

## RennTek Networking
The RennTek Network is a cross platform network API meant to be an easy-to-use solution for all developers.

## Currently supported client languages:
 - C#

## How to use RennTek Networking
### The server...
All RennTek Networking servers are hosted by RennTek themselves,
   if you want to host one yourself contact us and provide a thorough description
   as to why we can trust you, along with any relevant proof or projects
   
### The client...
#### C&#35;
The client namespace is entirely within RTNet, which means you'll
   have to be `using RTNet;` before accessing any of the client functions
   
In the simplest form, a client could be made of
```cs
using RTNet;
using System;

static void Main(string[] args)
{
   RTClient client = new RTClient("127.0.0.1", 4434); // Connect to 127.0.0.1:4434
   while(client.isConnected)
   {
     string line = Console.ReadLine();
     if(line.ToLower() == "quit")
       break;
   }
   client.Dispose(); //Get rid of any resources the client is using
}
```

## Images
<blockquote class="imgur-embed-pub" lang="en" data-id="Rc76W7X"><a href="//imgur.com/Rc76W7X">View post on imgur.com</a></blockquote><script async src="//s.imgur.com/min/embed.js" charset="utf-8"></script>



####RennTek Studios™ (2014-2016)

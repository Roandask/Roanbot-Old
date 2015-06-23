using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKYPE4COMLib;
using System.Reflection;
using ModuleBase;
using System.Collections;
using System.Timers;
using System.Diagnostics;


namespace Roanbot
{
    /// <summary>
    /// Handles incoming Skype messages and sends any commands to the correct module.
    /// </summary>
    class SkypeListener
    {
        private static Skype skype;
        private string trigger = "!";     // Say !help
        private string nick = "roanbot";  //Not used frequently, but easy to change when it is here.
        private string owner = "roandask2";
        private ArrayList commands = new ArrayList();
        private ArrayList objects = new ArrayList();
        private object helpcaller = null;   //This object will be set to BasicCommands when it is loaded. All additional commands
                                            //that are to be added to the help list will be sent here.
        private static Timer atimer;    //The timer for checking unread messages.

        /// <summary>
        /// Default SkypeListener constructor.
        /// nick will be "roanbot", owner will be "roandask2", trigger will be "!"
        /// </summary>
        public SkypeListener()
        {
            skype = new Skype();    //Create the Skype object and attach
            skype.Attach(8, false); //to the currently running application.
            skype.MessageStatus +=
              new _ISkypeEvents_MessageStatusEventHandler(skype_MessageStatus); //Add the message handler to the list of event functions.
            moduleLoader();

            atimer = new Timer(5000);               //Create the timer and direct it to
            atimer.Elapsed += this.checkUnread;     //checkUnread every 5 seconds.
            atimer.Enabled = true;
        }

        /// <summary>
        /// Sets the trigger, owner, and nick of the bot.
        /// </summary>
        /// <param name="newtrigger">Command trigger</param>
        /// <param name="newowner">Owner name</param>
        /// <param name="newnick">Bot's name</param>
        public SkypeListener(string newtrigger,string newowner, string newnick)
        {
            trigger = newtrigger;
            owner = newowner;
            nick = newnick;

            skype = new Skype();

            skype.Attach(8, false);
            skype.MessageStatus +=
              new _ISkypeEvents_MessageStatusEventHandler(skype_MessageStatus);
            moduleLoader();
        }

        /// <summary>
        /// Check for unread messages that Roanbot missed. Any messages sent more than 15 seconds ago will be ignored.
        /// Should only be called by the timer.
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="e">Provides data for the System.Timers.Timer.Elapsed event</param>
        private void checkUnread(Object source, ElapsedEventArgs e)
        {
            
            ChatMessageCollection missedMsgs = skype.MissedMessages;
            foreach (ChatMessage msg in missedMsgs)
            {
                double diff = DateTime.Today.Subtract(msg.Timestamp).TotalSeconds;  //Time difference
                Console.Write("Missed message: ");
                if (diff < 15)                              //Only process if the time difference is
                    skype_MessageStatus(msg, msg.Status);   //less than 15 seconds.
                else                                        //Otherwise, just mark it as read and move on.
                {
                    msg.Seen = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Loads modules from the Modules folder in the same directory as Roanbot.exe
        /// </summary>
        private void moduleLoader()
        {

            String path = Environment.CurrentDirectory; //Gets the current directory.
            path += "\\Modules";                        //We're looking for the Modules folder specifically.
            if (!System.IO.Directory.Exists(path))      //So, check to see if it exists.
            {
                System.IO.Directory.CreateDirectory(path);  //Otherwise... Create it, I guess. Roanbot won't be able to do mutch,
                                                            //but the folder will be there for later.
            }
            String[] modules = System.IO.Directory.GetFiles(path);  //Still, try to get the files in the folder.
            {
                String s = path + "\\BasicCommands.dll";            //Look first for BasicCommands, we want the help function.
                Console.WriteLine(s);
                if (!System.IO.File.Exists(s))                              //Check to see if it exists. If it doesn't,
                {
                    Console.WriteLine("Basic Commands does not exist!");    //Do nothing, because Roanbot can't do anything
                    Console.ReadLine();                                     //Without basic commands.
                    Environment.Exit(0);                                    //Wait for input, then exit.
                }
                else
                {
                    Assembly file1 = Assembly.LoadFile(s);              //If it is there, let's load BasicCommands.

                    Type[] type1 = file1.GetTypes();                    //Get the types and create BasicCommands
                    foreach (Type t in type1)
                    {
                        ConstructorInfo[] info = t.GetConstructors();
                        if (info.Count() == 0)
                            continue;
                        object dynam = info[0].Invoke(new object[] { });
                        ModuleBase.ModuleBase module = (ModuleBase.ModuleBase)dynam;    //We know BasicCommands extends ModuleBase
                        module.addReferences(this);                         //This calls the function that will cause the Module to add
                        Console.WriteLine("Loaded " + module.ToString());   //commands to the command list.
                    }
                }
            }
            foreach (String s in modules)       //We'll be loading all the .dll files in the folder.
            {
                if (s == path + "\\BasicCommands.dll") continue;    //Skip BasicCommands, we already loaded it.

                Assembly file = Assembly.LoadFile(s);   //Load the assembly file.
                Type[] type = file.GetTypes();          //Again, get all the types.
                foreach (Type t in type)
                {
                    try
                    {
                        ConstructorInfo[] info = t.GetConstructors();       //Not all the modules will only have 1 class.
                        if (info.Count() == 0)
                            continue;
                        Console.WriteLine("Loading... ");
                        object dynam = info[0].Invoke(new object[] { });                //If we try to invoke a constructor, the parameters
                        ModuleBase.ModuleBase module = (ModuleBase.ModuleBase)dynam;    //will be invalid - thus the try/catch.
                        module.addReferences(this);                                     //We can then cast the object
                        Console.WriteLine("Loaded " + module.ToString());               //and tell it to add the commands to our list.
                        
                    }
                    catch (System.Reflection.TargetParameterCountException)
                    {
                        Console.WriteLine("Not a module.");                             //It had an invalid constructor - it isn't a module.
                    }
                }
            }
        }

        /// <summary>
        /// Processes the messages recieved by Roanbot.
        /// </summary>
        /// <param name="msg">Skype message</param>
        /// <param name="status">Status of the message</param>
        private void skype_MessageStatus(ChatMessage msg, TChatMessageStatus status)
        {
            if (msg.Sender.Handle == nick)  //Ignore anything sent by Roanbot.
            {
                msg.Seen = true;
                return;
            }
            //If the message starts with the trigger, hasn't been read, and wasn't sent by Roanbot, go ahead and process it.
            else if (msg.Body.IndexOf(trigger) == 0 && status != TChatMessageStatus.cmsRead && status != TChatMessageStatus.cmsSent)
            {
                Console.WriteLine(msg.Body + " recieved from " + msg.Sender.Handle);    //Log the message in command line.
                String full = msg.Body.ToLower();                   //Easier to process in lower case.
                String[] command = full.Split(new char[] { ' ' });  //Split it by spaces - seperate arguments, usually.
                command[0] = command[0].Replace("!", "");           //Remove the trigger
                int index = commands.IndexOf(command[0]);           //Check the list of commands. -1 if not found.
                msg.Seen = true;
                if(msg.Sender.Handle.Equals(owner) && command[0].Equals("reload"))  //If the message sender is the owner,
                {                                                                   //And they want a reload - do it!
                    ProcessStartInfo Info = new ProcessStartInfo();
                    Info.Arguments = "/C TIMEOUT 1 && " + System.Reflection.Assembly.GetExecutingAssembly().Location;
                    Info.WindowStyle = ProcessWindowStyle.Normal;       //Timeout causes it to wait 1 second, then calls Roanbot.exe
                    Info.CreateNoWindow = false;
                    Info.FileName = "cmd.exe";
                    Process.Start(Info);
                    Environment.Exit(0);    //Exit - this should occur before the timeout is finished.
                    
                }
                else if (index < 0) //Negative index - command not listed.
                {
                    msg.Chat.SendMessage("I do not recognize that command.");
                }
                else
                {
                    ModuleBase.ModuleBase module = (ModuleBase.ModuleBase)(objects[index]); //No unusual cases, so we can send it
                    module.processCommands(skype, msg);                                     //to the relevant module.
                } 
            }
            //If it isn't a command but it IS new, still log it in the command line.
            else if (status != TChatMessageStatus.cmsRead && status != TChatMessageStatus.cmsSent)
            {
                Console.WriteLine(msg.Body + " recieved from " + msg.Sender.Handle);
            }
            msg.Seen = true; //We've seen the message!
        }

        /// <summary>
        /// Registers the command with Roanbot, sending it to the function that handles the help function.
        /// </summary>
        /// <param name="command">Name of the command to be added.</param>
        /// <param name="caller">The module to send the command to when received.</param>
        /// <param name="helpName">What to display in the help list. Should be the trigger followed by the command, or blank
        /// if the command is not to be listed.</param>
        public void addCommand(string command,object caller,String helpName) //TODO: Change helpName to a boolean.
        {
            Console.WriteLine("Recieved command " + command + " from " + caller.ToString());    //Log it in command line.
            commands.Add(command);      //Add it to the local list of commands.
            objects.Add(caller);
            if (command == "help")  //If the command is help (first to be loaded), register the object as the helpcaller.
            {
                helpcaller = caller;
                Type type = helpcaller.GetType();
                MethodInfo info = type.GetMethod("addHelp");
                info.Invoke(helpcaller, new object[] {helpName});   //We can now register commands with the help command.
            }
            else if (helpName != "")    //Only invoke addHelp if the helpName is not blank.
            {
                Type type = helpcaller.GetType();
                MethodInfo info = type.GetMethod("addHelp");
                info.Invoke(helpcaller, new object[] { helpName });
            }
        }

        //Not implemented. Might be for automatically pulling titles of imgur images, youtube videos, and such
        //to share with people in the conversation who might be interested.
        public void addAPI(string website, object caller)
        {
        }
    }
}

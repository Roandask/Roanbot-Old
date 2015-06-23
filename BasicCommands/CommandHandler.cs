using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModuleBase;
using System.Reflection;
using SKYPE4COMLib;
using System.Collections;

namespace BasicCommands
{
    public class CommandHandler : ModuleBase.ModuleBase
    {
        private ArrayList commands = new ArrayList();

        public CommandHandler()
        {
            
        }

        public override void addReferences(object obj)
        {
            Type type = obj.GetType();
            MethodInfo add = type.GetMethod("addCommand");
            add.Invoke(obj, new object[] { "help", this, "!help" });
            add.Invoke(obj, new object[] { "who", this, "!who" });
            add.Invoke(obj, new object[] { "echo", this, "!echo" });
            add.Invoke(obj, new object[] { "nesnes", this, "!nesnes" });
            add.Invoke(obj, new object[] { "yolo", this, "!yolo" });
            add.Invoke(obj, new object[] { "turgle", this, "!turgle" });
            add.Invoke(obj, new object[] { "ethos", this, "" });
        }

        public override void processCommands(Skype skype, ChatMessage msg)
        {
            String full = msg.Body.ToLower();
            String[] command = full.Split(new char[] { ' ' });
            command[0] = command[0].Replace("!", "");
            switch (command[0])
            {
                case "help":
                    msg.Chat.SendMessage("This message is being sent privately. If you are in a group chat, please check your private chat with me.");
                    String message = "The commands are:";
                    foreach (object obj in commands)
                    {
                        String s = (String)obj;
                        message += "\n" + s;
                    }
                    skype.SendMessage(msg.Sender.Handle, message);
                    break;
                case "who":
                    msg.Chat.SendMessage("Roandask created this bot using the SKYPE4COMLib API from Skype.\nBase code last modified: 6/22/15");
                    break;
                case "nesnes":
                    msg.Chat.SendMessage("H2O + H2O -> 2H2O");
                    break;
                case "yolo":
                    msg.Chat.SendMessage("%YOLO!");
                    break;
                case "turgle":
                    msg.Chat.SendMessage("WHO TURGLED?!");
                    break;
                case "ethos":
                    String args = "";
                    foreach (String s in command)
                    {
                        if (!s.Equals("ethos"))
                        {
                            args += " " + s;
                        }
                    }
                    if (args.Equals(" is the king of faggots"))
                        msg.Chat.SendMessage("This is now false.");
                    break;
                case "echo":
                    if (command.Length > 1)
                    {
                        string mssg = "";
                        for (int i = 1; i < command.Length; i++)
                        {
                            mssg += " " + command[i];
                        }
                        mssg = mssg.Trim();
                        msg.Chat.SendMessage(mssg);
                    }
                    break;
                default:
                    msg.Chat.SendMessage("How did this happen? What the fuck. This should not have happened.");
                    break;
            }
        }

        public override string ToString()
        {
            return "Basic Commands";
        }

        public void addHelp(String s)
        {
            commands.Add(s);
        }
    }
}

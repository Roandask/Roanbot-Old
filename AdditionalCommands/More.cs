using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModuleBase;
using SKYPE4COMLib;
using System.Reflection;

namespace AdditionalCommands
{
    public class More : ModuleBase.ModuleBase
    {
        public More()
        {
        }

        public override void addReferences(object obj)
        {
            Type type = obj.GetType();
            MethodInfo add = type.GetMethod("addCommand");
            add.Invoke(obj, new object[] { "ping", this, "!ping" });
            add.Invoke(obj, new object[] { "lastonline", this, "!lastonline" });
        }

        public override void processCommands(Skype skype, ChatMessage msg)
        {
            string full = msg.Body.ToLower().Replace("!", "");
            string[] command = full.Split(new char[] { ' ' });
            if(command[0] == "ping")
            {
                DateTime mytime = DateTime.Now;
                DateTime time = msg.Timestamp;
                time = time.ToLocalTime();
                TimeSpan difference = mytime - time;
                msg.Chat.SendMessage(difference.Milliseconds + " ms");
            }
            else if (command[0] == "lastonline")
            {
                if (command.Length < 2)
                {
                    msg.Chat.SendMessage("Must specify a user.");
                }
                else
                {
                    User user = skype.get_User((string)command[1]);

                    msg.Chat.SendMessage(user.FullName + " was last online " + user.LastOnline);
                }
            }
        }

        public override string ToString()
        {
            return "Additional Commands";
        }
    }
}

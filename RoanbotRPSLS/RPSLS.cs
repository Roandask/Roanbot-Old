using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SKYPE4COMLib;
using System.Collections;

namespace RoanbotRPSLS
{
    public class RPSLS : ModuleBase.ModuleBase
    {
        string[] choices = {"rock","paper","scissors","lizard","spock"};
        static ArrayList challanges = new ArrayList();

        public RPSLS()
        {
        }

        public override void addReferences(object obj)
        {
            Type type = obj.GetType();
            MethodInfo add = type.GetMethod("addCommand");
            add.Invoke(obj, new object[] { "rpsls", this, "!rpsls" });
        }

        public override void processCommands(Skype skype, ChatMessage msg)
        {
            String full = msg.Body.ToLower().Replace("!","");
            String[] command = full.Split(new char[] { ' ' });
            if (command.Count() < 2 || command[1] == "help")
            {
                string s = "The commands for Rock, Paper, Scissors, Lizard, Spock are:\nhelp\nchallenge\naccept\ndeny\nchoose\nchoices";
                msg.Chat.SendMessage(s);
            }
            else
            {
                switch (command[1])
                {
                    case "challenge":
                        challange(msg, command, skype);
                        break;
                    case "accept":
                        accept(msg, command, skype);
                        break;
                    case "deny":
                        deny(msg, command, skype);
                        break;
                    case "choose":
                        choose(msg, command, skype);
                        break;
                    case "choices":
                        getchoices(msg);
                        break;
                    default:
                        msg.Chat.SendMessage("I do not recognize your command.");
                        break;
                }
            }
        }

        public void getchoices(ChatMessage msg)
        {
            msg.Chat.SendMessage("Rock beats scissors and lizard.\nPaper beats rock and spock.\nScissors beats paper and lizard.\nLizard beats paper and spock.\nSpock beats rock and scissors.");
        }

        public void accept(ChatMessage msg, string[] command, Skype skype)
        {
            foreach (Challange challange in challanges)
            {
                if (challange.challanged == msg.Sender.Handle)
                {
                    challange.acceptChallange();
                    return;
                }
            }

            msg.Chat.SendMessage("You have not been challanged.");
        }

        public void deny(ChatMessage msg, string[] command, Skype skype)
        {
            foreach (Challange challange in challanges)
            {
                if (challange.challanged == msg.Sender.Handle)
                {
                    challange.denyChallange();
                    return;
                }
            }

            msg.Chat.SendMessage("You have not been challanged.");
        }

        public void challange(ChatMessage msg, string[] command, Skype skype)
        {
            foreach (Challange test in challanges)
            {
                if (test.challanger == msg.Sender.Handle || test.challanged == command[2])
                {
                    msg.Chat.SendMessage("That person is already in a challange.");
                    return;
                }
            }
            Challange challange = new Challange(msg.Sender.Handle, command[2], msg.ChatName, skype);
            challanges.Add(challange);
        }

        public void choose(ChatMessage msg, string[] command, Skype skype)
        {
            foreach (Challange challange in challanges)
            {
                if (challange.challanged == msg.Sender.Handle)
                {
                    if (challange.choice != -1)
                    {
                        msg.Chat.SendMessage("You have already voted.");
                        return;
                    }
                    int index = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (choices[i] == command[2])
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index == -1)
                    {
                        msg.Chat.SendMessage("Invalid choice.");
                        return;
                    }
                    challange.challangedVote(index);
                    int winner = challange.winner();
                    if (winner == -1)
                    {
                        challange.chat.SendMessage("The challange between " + skype.get_User(challange.challanger).FullName + " and " + skype.get_User(challange.challanged).FullName + " resulted in a draw. Both players chose " + choices[index] + ".");
                        RPSLS.deactivate(challange);
                    }
                    else if (winner == 0)
                    {
                        challange.chat.SendMessage(skype.get_User(challange.challanger).FullName + " won the challange between him and " + skype.get_User(challange.challanged).FullName + ", who chose " + choices[challange.choice] + ", with a vote of " + choices[challange.chalChoice] + ".");
                        RPSLS.deactivate(challange);
                    }
                    else if(winner == 1)
                    {
                        challange.chat.SendMessage(skype.get_User(challange.challanged).FullName + " won the challange between him and " + skype.get_User(challange.challanger).FullName + ", who chose " + choices[challange.chalChoice] + ", with a vote of " + choices[challange.choice] + ".");
                        RPSLS.deactivate(challange);
                    }
                    return;
                }
                else if (challange.challanger == msg.Sender.Handle)
                {
                    if (challange.chalChoice != -1)
                    {
                        msg.Chat.SendMessage("You have already voted.");
                        return;
                    }
                    int index = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (choices[i] == command[2])
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index == -1)
                    {
                        msg.Chat.SendMessage("Invalid choice.");
                        return;
                    }
                    else
                    {
                        msg.Chat.SendMessage("Your choice has been counted.");
                    }
                    challange.challangerVote(index);
                    int winner = challange.winner();
                    if (winner == -1)
                    {
                        challange.chat.SendMessage("The challange between " + skype.get_User(challange.challanger).FullName + " and " + skype.get_User(challange.challanged).FullName + " resulted in a draw. Both players chose " + choices[index] + ".");
                        RPSLS.deactivate(challange);
                    }
                    else if (winner == 0)
                    {
                        challange.chat.SendMessage(skype.get_User(challange.challanger).FullName + " won the challange between him and " + skype.get_User(challange.challanged).FullName + ", who chose " + choices[challange.choice] + ", with a vote of " + choices[challange.chalChoice] + ".");
                        RPSLS.deactivate(challange);
                    }
                    else if(winner == 1)
                    {
                        challange.chat.SendMessage(skype.get_User(challange.challanged).FullName + " won the challange between him and " + skype.get_User(challange.challanger).FullName + ", who chose " + choices[challange.chalChoice] + ", with a vote of " + choices[challange.choice] + ".");
                        RPSLS.deactivate(challange);
                    }
                    return;
                }
            }
        }

        public static void deactivate(Challange challange)
        {
            challanges.Remove(challange);
        }
    }
}

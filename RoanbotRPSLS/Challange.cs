using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKYPE4COMLib;

namespace RoanbotRPSLS
{
    public class Challange
    {
        public String challanger;
        public String challanged;
        public String chatName;
        public Chat chat;
        public Skype skype;
        public bool started = false;
        public int chalChoice = -1;
        public int choice = -1;

        public Challange(String challanger, String challanged, String chatID, Skype skype)
        {
            this.challanged = challanged;
            this.challanger = challanger;
            chatName = chatID;
            this.skype = skype;
            chat = skype.get_Chat(chatID);
            chat.SendMessage(skype.get_User(challanger).FullName + " has challanged " + skype.get_User(challanged).FullName + " to a game of Rock, Paper, Scissors, Lizard, Spock! " + skype.get_User(challanged).FullName + ", do you accept?");
        }

        public void acceptChallange()
        {
            chat.SendMessage(skype.get_User(challanged).FullName + " has accepted the challange! Both players, please PM me your choice with '!RPSLS choose <choice>'.");
            ready();
        }

        public void denyChallange()
        {
            chat.SendMessage(skype.get_User(challanged).FullName + " has denied the challange!");
            RPSLS.deactivate(this);
        }

        public void ready()
        {
            started = true;
        }

        public void challangerVote(int choice)
        {
            chalChoice = choice;
        }

        public void challangedVote(int choice)
        {
            this.choice = choice;
        }

        public bool voted()
        {
            if (chalChoice == -1 || choice == -1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public int winner() //-1 draw, 0 challanger, 1 challanged
        {
            switch(chalChoice)
            {
                case 0:
                    if (choice == 1 || choice == 4)
                    {
                        return 1;
                    }
                    else if (choice == 2 || choice == 3)
                    {
                        return 0;
                    }
                    break;
                case 1:
                    if (choice == 2 || choice == 3)
                    {
                        return 1;
                    }
                    else if (choice == 0 || choice == 4)
                    {
                        return 0;
                    }
                    break;
                case 2:
                    if (choice == 0 || choice == 4)
                    {
                        return 1;
                    }
                    else if (choice == 1 || choice == 3)
                    {
                        return 0;
                    }
                    break;
                case 3:
                    if (choice == 0 || choice == 2)
                    {
                        return 1;
                    }
                    else if (choice == 1 || choice == 4)
                    {
                        return 0;
                    }
                    break;
                case 4:
                    if (choice == 1 || choice == 3)
                    {
                        return 1;
                    }
                    else if (choice == 0 || choice == 2)
                    {
                        return 0;
                    }
                    break;
            }
            return -2; //none
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            Challange check = (Challange)obj;
            if (check.challanger == this.challanger && check.challanged == this.challanged && check.chatName == this.chatName)
            {
                return true;
            }
            return false;
        }
    }
}

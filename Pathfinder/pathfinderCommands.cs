using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModuleBase;
using System.Reflection;
using SKYPE4COMLib;
using System.Collections;

namespace Pathfinder
{
    /// <summary>
    /// Registers commands to Roanbot and processes the commands it gets sent. Should only be sent commands it registered.
    /// Extends ModuleBase.
    /// </summary>
    class PathfinderCommands : ModuleBase.ModuleBase
    {
        private static Random rand = new Random();      //This rolls the dice (surprise surprise!)

        /// <summary>
        /// Invokes the main class' addCommand method with commands this class will handle.
        /// </summary>
        /// <param name="obj">The main class that handles Skype messages.</param>
        public override void addReferences(object obj)  //This method is required by Roanbot so that it can load generic modules without modifying base code.
        {
            Type type = obj.GetType();
            MethodInfo add = type.GetMethod("addCommand");              //addCommand in the main class registers all commands, hidden or listed, with the main program.
            add.Invoke(obj, new object[] { "roll", this, "!roll" });    //Passes the command, this object (so it can pass the command), and sets the shown command.
                                                                            //Note: Would probably be better as a boolean rather than a string, as the third
                                                                            //parameter will always be !<command name>
            add.Invoke(obj, new object[] { "r", this, "" });            //A blank call sign means it should not show up on the help list.
                                                                            //Again, could just be a boolean as not showing up is always a blank string.
        }

        /// <summary>
        /// Processes a Skype message command. This function should only be run if the command is registered by the class, otherwise
        /// it may not function correctly.
        /// </summary>
        /// <param name="skype">The current Skype instance.</param>
        /// <param name="msg">The message to be processed by the function.</param>
        public override void processCommands(Skype skype, ChatMessage msg)
        {
            String full = msg.Body.ToLower().Replace("!", "");                              //Sets it to lower case and removes the trigger for simplicity.
            full = System.Text.RegularExpressions.Regex.Replace(full,"\\[(.*?)\\]","");     //Remove the inline labels from the roll.
            if (full.Contains('\\'))                                                        //Checks to see if the \ character appears in the roll.
                full = full.Remove(full.IndexOf('\\'));                                     //If it does, remove all text following it.
            String[] command = full.Split(new char[] { ' ' });                              //Split the string into multiple parts.
            if (command.Count() < 2 || command[1] == "help")                                //Conditions for displaying help.
            {
                string s = "Usage: XdY + Z, any amount of dice/constants. Supports addition, subtraction, multiplication, division, and parentheses.\nInline text: 2d6[cold] + 3d6 [bludgeoning].\nEnd of command text: 1d10 + 1 \\This will be ignored: +5";
                msg.Chat.SendMessage(s);
            }
            else
            {
                string input = command[1];                  //Condenses all the rest of the parameters to one string
                for (int i = 2; i < command.Length; i++)    //with no spaces.
                {
                    input += command[i];
                }

                String finalS = "";


                Queue<string> output = new Queue<string>(); //Sets up the Queue and Stack for converting
                Stack<char> stack = new Stack<char>();      //to Reverse Polish Notation.

                int dump;   //Only used for TryParse
                string currentNum = "";
                foreach (char c in input)
                {
                    //Keep adding each digit to the current number until we have an operator.
                    if (Int32.TryParse(c + "", out dump))
                    {
                        currentNum += c;
                    }
                    else if (c == '.')
                    {
                        currentNum += c;
                    }
                    else
                    {
                        if(currentNum != "")            //If we have a value, add it to the queue.
                            output.Enqueue(currentNum);
                        currentNum = "";                //And reset the currentNum.
                        int prec = precedence(c);       //Check the precedence of the operator: -1 if it is not valid, or for parentheses.
                        if (c == '(')                   //If it is an opening parenthesis all we need to do is push it to the stack.
                        {
                            stack.Push(c);
                        }
                        else if (c == ')')              //For closing ones, we pop all of the operators on the stack and move them to the queue.
                        {
                            char pops = stack.Pop();
                            while (pops != '(')         //However, we don't want to add parentheses to the queue.
                            {
                                output.Enqueue(pops + "");
                                if (stack.Count == 0)   //If we reached the end of the stack and still haven't gotten the opening parenthesis
                                {                       //Then there are mismatched parentheses.
                                    msg.Chat.SendMessage("Invalid roll: mismatched parentheses.");
                                    return;
                                }
                                pops = stack.Pop();
                            }

                        }
                        else if (prec == -1)            //Any characters that still have -1 precedence are invalid.
                        {
                            msg.Chat.SendMessage("Invalid roll: unsupported character: " + c);
                            return;
                        }
                        else if (stack.Count == 0 || precedence(stack.Peek()) < prec)
                        {
                            stack.Push(c);              //Push if the operator has a higher precedence than anything on the stack.
                        }
                        else
                        {
                            output.Enqueue(stack.Pop() + "");   //If the precedence is less than or equal to the top of the stack,
                            stack.Push(c);                      //We pop the stack and add the operator to the queue. Then push the new operator.
                        }                                       //Thus, the operator on the stack will get evaluated before the new operator.
                                                                //So if the stack was *, and the new operator is +, the multiplication gets evaluated first.
                    }
                }
                if (currentNum != "")               //Check if there is a number to add to the queue
                    output.Enqueue(currentNum);     //at the end of the expression.
                while (stack.Count != 0)            //Then, pop all of the operators onto the queue.
                {
                    char pops = stack.Pop();
                    if (pops == '(')                //If we find a parenthesis still on the stack, we have mismatched parentheses.
                    {
                        msg.Chat.SendMessage("Invalid roll: mismatched parentheses.");
                        return;
                    }
                    output.Enqueue(pops + "");
                }
                Stack<double> values = new Stack<double>();     //Set up a new stack for the values.
                while (output.Count > 0)
                {
                    String current = output.Dequeue();
                    double val;
                    if (Double.TryParse(current, out val))      //We send values immediately to the stack.
                    {
                        values.Push(val);
                    }
                    else
                    {
                        if (values.Count < 2)                   //All operators should have 2 arguments in this usage.
                        {                                       //If it doesn't, someone didn't put enough numbers!
                            msg.Chat.SendMessage("Invalid roll: Not enough values in expression.");
                            return;
                        }
                        double a = values.Pop();        //Pop the values we're going to be working with.
                        double b = values.Pop();        //The expression will be "b OP a" since this is a stack.
                        if (current.Equals("+"))
                            values.Push(b + a);
                        else if (current.Equals("-"))
                            values.Push(b - a);
                        else if (current.Equals("*"))
                            values.Push(b * a);
                        else if (current.Equals("/"))
                            values.Push(b / a);
                        else if (current.Equals("d"))
                        {
                            int n = (int)b;     //Round down for the dice rolls!
                            int s = (int)a;
                            double subtotal = 0;    //Keeps track of the rolls and the subtotal for this dice roll.
                            String substring = n > 1 ? "The rolls for " + n + "d" + s + " were:" : "The roll for " + n + "d" + s + " was:";
                            for (int i = 0; i < n; i++)
                            {
                                int roll = rand.Next(s) + 1;
                                substring += " " + roll;
                                subtotal += roll;
                            }
                            substring += "\nThe subtotal is: " + subtotal;
                            finalS += substring + "\n\n";   //The finalS will be sent at the end assuming no problems occur.
                            values.Push(subtotal);  //Now push the subtotal so that it can be used by other operations.
                        }
                    }
                }

                if (values.Count > 1)   //There should only be 1 value on the stack: The result.
                {                       //Anything else is too many values... ex "!r 11(1+2)" instead of "!r 11*(1+2)"
                    msg.Chat.SendMessage("Invalid roll: Too many input values.");
                    return;
                }

                string send = finalS + "The total roll is: " + values.Pop() + ".";
                msg.Chat.SendMessage(send); //Now display the result!
            }
        }

        /// <summary>
        /// Returns the precedence of supported operations, or -1 if the operator is not supported.
        /// </summary>
        /// <param name="s">The operatoion character.</param>
        /// <returns>The relative precedence of the operation. Addition/Subtraction are 2, Multiplication/Division are 3, and a Dice roll is 4. Otherwise returns -1.</returns>
        private int precedence(char s)
        {
            switch (s)
            {
                case '+': return 2;
                case '-': return 2;
                case '*': return 3;
                case '/': return 3;
                case 'd': return 4;
                default: return -1;
            }
        }
    }
}

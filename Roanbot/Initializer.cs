using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKYPE4COMLib;
using System.IO;

namespace Roanbot
{
    class Initializer
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Checking for updates...");
            String path = Environment.CurrentDirectory;
            if (Directory.Exists(path + "\\Updates"))
            {
                String[] modules = Directory.GetFiles(path + "\\Updates");
                if (modules.Length == 0)
                    Console.WriteLine("No Updates.");
                else
                {
                    foreach (String s in modules)
                    {
                        Console.WriteLine("Moving " + s + " to Modules.");
                        File.Copy(s, s.Replace(path+"\\Updates\\",path+"\\Modules\\"), true);
                        File.Delete(s);
                    }
                    Console.WriteLine("Updates Finished.");
                }
            }
            else
                Console.WriteLine("No updates.");


            Console.WriteLine("Starting bot...");
            SkypeListener listener = new SkypeListener();
            Console.WriteLine("Bot started.");
            Console.ReadLine();
            
        }


    }
}

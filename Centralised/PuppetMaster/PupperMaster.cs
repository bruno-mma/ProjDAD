using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Program
    {
        static void Main()
        {
            if (File.Exists("configFile.txt"))
            {
                try
                {
                    string[] configFileContents = File.ReadAllLines("configFile.txt");

                    // TODO: make the puppet master execute the commands
                    // for now it just prints them
                    foreach (var item in configFileContents)
                    {
                        Console.WriteLine(item);
                    }
                    Console.ReadLine();
                }
            
                catch (IOException e) {
                    Console.WriteLine(e);
                }
            }

            else
            {
                while (true)
                {
                    string command = Console.ReadLine();
                    // TODO: execute command
                }
            }
        }
    }

    public class PuppetMaster
    {
        public PuppetMaster ()
        {

        }
    }
}

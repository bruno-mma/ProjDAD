using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace PuppetMaster {

    class Program {
    
        static void Main() {
        
            try {
                // File needs to be inside ...\PupperMaster\bin\Debug
                // TODO: find how to give the file path without having to be an absolute path
                string[] configFileContents = File.ReadAllLines("configFile.txt");

                foreach (var item in configFileContents) {
                    // TODO: execute command
                }
                Console.ReadLine();
            }
            
            catch (FileNotFoundException) {
                Console.WriteLine("No configuration file provided. Reading commands from console");
                    
                while (true) {
                    string command = Console.ReadLine();
                    // TODO: execute command
                }
            }
        }
    }

    public class PuppetMaster {
        // just as an example
        private readonly List<string> _availablePCSs = new List<string> {
            "tcp://URL:10000", 
            "tcp://URL2:10000" 
        };

        public PuppetMaster () {
        }
    }
}

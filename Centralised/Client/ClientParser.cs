using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	//this class will parse and execute Client commands, both from console input and ScriptClient files
	class ClientParser
	{

		private Client _client;

		public ClientParser(Client client)
		{
			_client = client;
		}

		public void Parse(string command)
		{
			List<string> arguments = new List<string>(command.Split(' '));

			//remove empty strings from argument list
			arguments = arguments.Where(argument => !string.IsNullOrWhiteSpace(argument)).ToList();

			//ignore empty lines
			if (arguments.Count == 0)
			{
				return;
			}

			switch (arguments[0])
			{
				case "quit":
				case "exit":
					Environment.Exit(0);
					break;


				case "connect":
					Connect(arguments);
					break;


				default:
					Console.WriteLine("Error: " + arguments[0] + " command not found");
					break;
			}
		}

		private void PrintErrorMessage(string command)
		{
			Console.WriteLine("ClientParser error: Error parsing command " + command);
		}

		private bool CheckArgumentCount(List<string> arguments, int correctCount)
		{
			if (arguments.Count != correctCount)
			{
				PrintErrorMessage(arguments[0]);
				return false;
			}

			return true;
		}

		private void Connect(List<string> arguments)
		{
			if (!CheckArgumentCount(arguments, 2))
			{
				return;
			}

			_client.Connect(arguments[1]);
			Console.WriteLine("Connected as user: " + arguments[1]);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PuppetMaster
{
	class PuppetMasterParser
	{
		private PuppetMaster _puppetMaster;

		public PuppetMasterParser(PuppetMaster puppet_master)
		{
			_puppetMaster = puppet_master;
		}

		private void PrintErrorMessage(string command)
		{
			Console.WriteLine("PuppetMasterParser, Error parsing command " + command);
		}

		//check if command has a minimum number of arguments
		private bool WrongArgumentCount(List<string> arguments, int correctCount)
		{
			if (arguments.Count - 1 < correctCount)
			{
				PrintErrorMessage(arguments[0]);
				return true;
			}

			return false;
		}


		public void ParseExecute(string command)
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

				case "Client":
					StartClient(arguments);
					break;

				case "Server":
					StartServer(arguments);
					break;

				case "AddRoom":
					AddRoom(arguments);
					break;

				//AddPCS IP
				case "AddPCS":
					AddPCS(arguments);
					break;

				case "Wait":
					Wait(arguments);
					break;

				case "Freeze":
					Freeze(arguments);
					break;

				case "Unfreeze":
					Unfreeze(arguments);
					break;

				default:
					Console.WriteLine("Error: " + arguments[0] + " command not found");
					break;
			}
		}

		private void StartClient(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 4)) return;

			_puppetMaster.StartClient(arguments[1], arguments[2], arguments[3], arguments[4]);
		}

		private void StartServer(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 5)) return;

			_puppetMaster.StartServer(arguments[1], arguments[2], Int32.Parse(arguments[3]), Int32.Parse(arguments[4]), Int32.Parse(arguments[5]));
		}

		private void AddRoom(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 3)) return;

			_puppetMaster.AddRoom(arguments[1], arguments[3], Int32.Parse(arguments[2]));
		}

		private void Wait(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			Thread.Sleep(Int32.Parse(arguments[1]));
		}

		private void AddPCS(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			_puppetMaster.AddPCS(arguments[1]);
		}

		private void Freeze(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			_puppetMaster.FreezeServer(arguments[1]);
		}

		private void Unfreeze(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			_puppetMaster.UnfreezeServer(arguments[1]);
		}
	}
}

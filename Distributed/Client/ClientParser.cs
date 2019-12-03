using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

			switch (arguments[0].ToLower())
			{
				case "quit":
				case "exit":
					Environment.Exit(0);
					break;

				case "connect":
					Connect(arguments);
					break;

				case "create":
					Create(arguments);
					break;

				case "join":
					Join(arguments);
					break;

				case "list":
					List();
					break;

				case "close":
					Close(arguments);
					break;

				case "wait":
					Wait(arguments);
					break;

				case "run":
					RunScript(arguments, false);
					break;

				case "runp":
					RunScript(arguments, true);
					break;

				default:
					Console.WriteLine("Error: " + arguments[0] + " command not found");
					break;
			}
		}

		private void PrintErrorMessage(string command)
		{
			Console.WriteLine("ClientParser, Error parsing command " + command);
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


		private void Connect(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 3)) return;

			_client.Connect(arguments[1], arguments[2], arguments[3]);
		}

		private void Create(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 5)) return;

			int number_of_slots = Int32.Parse(arguments[3]);
			int number_of_invitees = Int32.Parse(arguments[4]);

			//correct number of arguments counting with the number of slots and invitees
			if (WrongArgumentCount( arguments, 4 + number_of_slots + number_of_invitees) ) return;

			List<string> slots = arguments.GetRange(5, number_of_slots);
			List<string> invitees = null;

			if (number_of_invitees > 0)
			{
				invitees = arguments.GetRange(5 + number_of_slots, number_of_invitees);
			}

			string topic = arguments[1];
			int min_attendees = Int32.Parse(arguments[2]);

			_client.CreateMeeting(topic, min_attendees, number_of_slots, number_of_invitees, slots, invitees);
		}

		private void Join(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 3)) return;

			int number_of_slots = Int32.Parse(arguments[2]);

			//correct number of arguments counting with the number of slots and invitees
			if (WrongArgumentCount(arguments, 2 + number_of_slots)) return;

			List<string> slots = arguments.GetRange(3, number_of_slots);

			_client.Join(arguments[1], number_of_slots, slots);
		}

		private void List()
		{
			_client.List();
		}

		private void Close(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			_client.CloseMeeting(arguments[1]);
		}

		private void Wait(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			Thread.Sleep(Int32.Parse(arguments[1]));
		}

		public void RunScript(List<string> arguments, bool pause)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			try
			{
				//assuming script is inside the solution folder
				string[] lines = System.IO.File.ReadAllLines(@"..\..\..\" + arguments[1] + ".txt");

				foreach (string line in lines)
				{
					if (pause)
					{
						Console.WriteLine("Press enter to run Command:" + line);
						Console.ReadLine();
					}
					else
					{
						Console.WriteLine("Running Command: " + line);
					}

					this.ParseExecute(line);
					Console.WriteLine();
				}

				Console.Write("Finished script execution.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.Write("ClientParser: " + arguments[1] + ".txt File not found.");
			}

			Console.WriteLine(" Reading commands from console");
		}
	}
}

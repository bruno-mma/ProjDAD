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

				case "create":
					Create(arguments);
					break;

				case "join":
					Join(arguments);
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
			if (WrongArgumentCount(arguments, 1)) return;

			//TODO: check if connection was successful
			_client.Connect(arguments[1]);
			Console.WriteLine("Connected as user: " + arguments[1]);
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
				invitees = arguments.GetRange(6 + number_of_slots, number_of_invitees);
			}

			string topic = arguments[1];
			int min_attendees = Int32.Parse(arguments[2]);

			_client.CreateMeeting(topic, min_attendees, number_of_slots, number_of_invitees, slots, invitees);
		}

		private void Join(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 3)) return;

			int number_of_slots = Int32.Parse(arguments[3]);

			//correct number of arguments counting with the number of slots and invitees
			if (WrongArgumentCount(arguments, 2 + number_of_slots)) return;

			List<string> slots = arguments.GetRange(3, number_of_slots);

			_client.Join(arguments[1], number_of_slots, slots);
		}

	}
}

using Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Client
{
	public class Program
	{
		static void Main(string[] args)
		{
			Client client = new Client();
			ClientParser parser = new ClientParser(client);

			while (true)
			{
				parser.Parse(Console.ReadLine());
			}
		}
	}

	public class Client : MarshalByRefObject, IClient
	{
		private readonly static int _initialPort = 9000;

		private string _name;
		private IServer _server;

		private Dictionary<string, MeetingData> _knownMeetings = new Dictionary<string, MeetingData>();

		public Client()
		{
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		public void Connect(string name)
		{
			int port = _initialPort;
			bool open_port_found = false;

			while (!open_port_found)
			{
				try
				{
					TcpChannel channel = new TcpChannel(port);
					ChannelServices.RegisterChannel(channel, false);
					open_port_found = true;
				}
				catch (System.Net.Sockets.SocketException)
				{
					port++;
				}
			}


			//get remote server object
			IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://localhost:8080/Server");

			this._name = name;

			//weak check
			if (server == null)
			{
				Console.WriteLine("Could not locate server");
				return;
			}

			_server = server;

			//publish remote client object
			RemotingServices.Marshal(this, name, typeof(IClient));

			server.AddClient(name, port);
		}

		public void UpdateMeeting(string meeting_topic, MeetingData meetingData)
		{
			//(Distributed version) Aditional logic will needed to determine if this meeting data is actualy more recent than the one already saved
			_knownMeetings[meeting_topic] = meetingData;

			string slots = "slots ";

			foreach (KeyValuePair<string, List<string>> kvp in meetingData._meetingRecords)
			{
				string slot = kvp.Key;
				List<string> users = kvp.Value;

				slots += slot + ": ";

				foreach (string user in users)
				{
					slots += user + " ";
				}

				slots += Environment.NewLine;
			}
		}

		public void CreateMeeting(string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			bool successful = _server.CreateMeeting(_name, meeting_topic, min_attendees, number_of_slots, number_of_invitees, slots, invitees);

			if (successful)
			{
				string print = "Created a meeting with topic: " + meeting_topic + ", with " + min_attendees + " required atendees, with slots: ";

				foreach (string slot in slots)
				{
					print += slot + " ";
				}

				if (number_of_invitees < 0)
				{
					print += "and invitees: ";

					foreach (string invitee in invitees)
					{
						print += invitee + " ";
					}
				}

				Console.WriteLine(print);
			}

			else
			{
				Console.WriteLine("Error: Meeting creation failed");
			}
		}

		public void Join(string meeting_topic, int number_of_slots, List<string> slots)
		{
			bool successful = _server.JoinMeeting(_name, meeting_topic, number_of_slots, slots);

			if (successful)
			{
				string print = "Joined meeting with topic: " + meeting_topic + ".";
				Console.WriteLine(print);
			}

			else
			{
				Console.WriteLine("Error: Cannot join meeting");
			}
		}

		public void List()
		{
			foreach (MeetingData meetingData in _knownMeetings.Values)
			{
				Console.WriteLine("Meeting topic: " + meetingData._meetingTopic + ", coordinator: " + meetingData._meetingOwner);

				if (meetingData._closed)
				{
					Console.WriteLine("===CLOSED===");
					Console.WriteLine("Scheduled at " + meetingData._selectedDate + " at room " + meetingData._selectedRoom + " selected to attend:");

					string attending_users = "";
					foreach (string  user in meetingData._selectedUsers)
					{
						attending_users += user + ' ';
					}
					Console.WriteLine(attending_users);
				}

				Console.WriteLine(meetingData._minAttendees + " minimum attendees, " + meetingData._numberOfSlots + " slots.");

				if (meetingData._numberOfInvitees > 0)
				{
					Console.WriteLine(meetingData._numberOfInvitees + " invitees:");

					string invitees = "";

					foreach (string invitee in meetingData._invitees)
					{
						invitees += invitee + " ";
					}

					Console.WriteLine(invitees);
				}

				Console.WriteLine("Slots:");

				string slots = "";

				foreach (KeyValuePair<string, List<string>> kvp in meetingData._meetingRecords)
				{
					string slot = kvp.Key;
					List<string> users = kvp.Value;

					slots += "-" + slot + ": ";

					foreach (string user in users)
					{
						slots += user + " ";
					}

					slots += Environment.NewLine;
				}

				Console.WriteLine(slots);
			}
		}

		public void CloseMeeting(string meeting_topic)
		{
			bool successful = _server.CloseMeeting(_name, meeting_topic);
			MeetingData meetingData = _knownMeetings[meeting_topic];

			if (successful)
			{
				string print = "Successfully closed meeting " + meeting_topic + " at room " + meetingData._selectedRoom + " at date " + meetingData._selectedDate + ", with users:";
				Console.WriteLine(print);

				string users = "";
				foreach (string user in meetingData._selectedUsers)
				{
					users += user + " ";
				}
				Console.WriteLine(users);
			}
			else
			{
				Console.WriteLine("Error: Cannot close meeting");
			}
		}
	}
}

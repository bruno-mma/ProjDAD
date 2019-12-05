using Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace Client
{
	public class Program
	{
		static void Main(string[] args)
		{
			Client client = new Client();
			ClientParser parser = new ClientParser(client);

			//if started by PuppetMaster
			if (args.Length != 0)
			{
				client.Connect(args[0], args[1], args[2]);
				Console.WriteLine("Executing script: " + args[3] + ".txt");
				parser.RunScript( new List<string>(){ "run", args[3] } , false);
			}

			while (true)
			{
				parser.ParseExecute(Console.ReadLine());
				Console.WriteLine();
			}
		}
	}

	public class Client : MarshalByRefObject, IClient
	{
		private readonly string clientURLsPath = @"..\..\..\" + "clientURLs.txt";

		private string _name;
		private IServer _server;

		private Dictionary<string, MeetingData> _knownMeetings = new Dictionary<string, MeetingData>();

		public override object InitializeLifetimeService()
		{
			return null;
		}

		private void AddClientURLToFile(string client_URL)
		{
			using (StreamWriter sw = File.AppendText(clientURLsPath))
			{
				sw.WriteLine(client_URL);
			}
		}


		public void Connect(string name, string user_URL, string server_URL)
		{
			this._name = name;

			TcpChannel channel = new TcpChannel(URL.GetPort(user_URL));
			ChannelServices.RegisterChannel(channel, false);

			//get remote server object
			IServer server = (IServer)Activator.GetObject(typeof(IServer), server_URL);

			//weak check
			if (server == null)
			{
				Console.WriteLine("Could not locate server at " + server_URL);
				return;
			}

			_server = server;

			//publish remote client object
			RemotingServices.Marshal(this, URL.GetURI(user_URL), typeof(IClient));

			server.AddClient(user_URL, _name);

			Console.WriteLine("Connected as user " + name + " to server at " + server_URL);

			AddClientURLToFile(user_URL);
		}

		public void UpdateMeeting(string meeting_topic, MeetingData meetingData)
		{
			//(Distributed version) Aditional logic will needed to determine if this meeting data is actualy more recent than the one already saved
			Console.WriteLine("Got a new meeting: " + meeting_topic);

			_knownMeetings[meeting_topic] = meetingData;
		}

		public void CreateMeeting(string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			Console.WriteLine(_server.CreateMeeting(_name, meeting_topic, min_attendees, number_of_slots, number_of_invitees, slots, invitees));
		}

		public void Join(string meeting_topic, int number_of_slots, List<string> slots)
		{
			Console.WriteLine(_server.JoinMeeting(_name, meeting_topic, number_of_slots, slots));
		}

		//TODO: print something different if meeting was canceled
		public void List()
		{
			List<string> meeting_topics = new List<string>(_knownMeetings.Keys);

			//update meeting information before listing
			foreach (string meeting_topic in meeting_topics)
			{
				this._knownMeetings[meeting_topic] = _server.GetUpdatedMeeting(meeting_topic);
			}

			Console.WriteLine("----------------------------------------LIST----------------------------------------");


			foreach (MeetingData meetingData in _knownMeetings.Values)
			{
				Console.WriteLine("Meeting topic: " + meetingData._meetingTopic + ", coordinator: " + meetingData._meetingOwner);
				Console.WriteLine(meetingData._minAttendees + " minimum attendees, " + meetingData._numberOfSlots + " slots.");

				if (meetingData._numberOfInvitees > 0)
				{
					Console.Write(meetingData._numberOfInvitees + " invitees: ");

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
				Console.Write(slots);

				if (meetingData._closed)
				{
					Console.WriteLine("======CLOSED======");
					Console.WriteLine("Scheduled at " + meetingData._selectedDate + " room " + meetingData._selectedRoom + " selected to attend:");

					string attending_users = "";
					foreach (string  user in meetingData._selectedUsers)
					{
						attending_users += user + ' ';
					}
					Console.WriteLine(attending_users);
				}

				Console.WriteLine();
			}
		}

		public void CloseMeeting(string meeting_topic)
		{
			Console.WriteLine(_server.CloseMeeting(_name, meeting_topic));
		}
	}
}

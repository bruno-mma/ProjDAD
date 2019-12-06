﻿using Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
			string name = args[0];
			string my_URL = args[1];
			string server_URL = args[2];
			string script = args[3];

			int port = URL.GetPort(my_URL);
			string URI = URL.GetURI(my_URL);

			Client client = new Client(name, my_URL);
			ClientParser parser = new ClientParser(client);

			TcpChannel channel = new TcpChannel(port);
			ChannelServices.RegisterChannel(channel, false);

			//publish remote client object
			RemotingServices.Marshal(client, URI, typeof(IClient));

			client.Connect(server_URL);
			Console.WriteLine("Executing script: " + script + ".txt");
			parser.RunScript( new List<string>(){ "run", script } , false);
			

			while (true)
			{
				parser.ParseExecute(Console.ReadLine());
				Console.WriteLine();
			}
		}
	}

	public class Client : MarshalByRefObject, IClient
	{
		private string _name;
		private string _MyURL;

		private IServer _server;
		private string _serverURL;

		private Dictionary<string, MeetingData> _knownMeetings = new Dictionary<string, MeetingData>();

		//URLs of known servers
		public HashSet<string> _knownServers = new HashSet<string>();

		//keep track of offline servers
		public HashSet<string> _offlineServers = new HashSet<string>();

		public override object InitializeLifetimeService()
		{
			return null;
		}

		public Client(string name, string URL)
		{
			_name = name;
			_MyURL = URL;
		}

		public void Connect(string server_URL)
		{
			//get remote server object
			IServer server = (IServer)Activator.GetObject(typeof(IServer), server_URL);

			//weak check
			if (server == null)
			{
				Console.WriteLine("Could not locate server at " + server_URL);
				return;
			}

			_server = server;
			_serverURL = server_URL;

			//publish remote client object
			RemotingServices.Marshal(this, URL.GetURI(_MyURL), typeof(IClient));

			string client_URL = "tcp://" + GetLocalIPAddress() + ":" + URL.GetPort(_MyURL) + "/" + URL.GetURI(_MyURL);

			Console.WriteLine("Connecting as URL: " + client_URL);

			server.AddClient(client_URL, _name);

			Console.WriteLine("Connected as user " + _name + " to server at " + server_URL);
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

				if (meetingData._canceled)
				{
					Console.WriteLine("======CANCELED======");
				}

				else if (meetingData._closed)
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

		public static string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}

		public void UpdateKnownServers(List<string> server_URLs)
		{
			server_URLs.Remove(_serverURL);

			foreach (string URL in server_URLs)
			{
				if (!_knownServers.Contains(URL) && !_offlineServers.Contains(URL))
				{
					Console.WriteLine("Got word of a new server at " + URL);
					_knownServers.Add(URL);
				}
			}
		}
	}
}

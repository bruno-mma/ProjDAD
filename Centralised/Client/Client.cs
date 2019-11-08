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
		}

		public void CreateMeeting(string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			_server.CreateMeeting(_name, meeting_topic, min_attendees, number_of_slots, number_of_invitees, slots, invitees);
		}
	}
}

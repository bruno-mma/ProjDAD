using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class Program
	{
		public static readonly int _server_port = 8080;

		static void Main(string[] args)
		{
			TcpChannel channel = new TcpChannel(_server_port);
			ChannelServices.RegisterChannel(channel, false);

			Server server = new Server();
			RemotingServices.Marshal(server, "Server", typeof(IServer));

			Console.ReadLine();
		}
	}


	public class Server : MarshalByRefObject, IServer
	{
		//key is meeting topic
		private Dictionary<string, Meeting> _meetings = new Dictionary<string, Meeting>();

		//key is client name
		private Dictionary<string, IClient> _clients = new Dictionary<string, IClient>();


		public void AddClient(string client_name, int port)
		{
			IClient client = (IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:" + port + "/" + client_name);

			//weak check
			if (client == null)
			{
				Console.WriteLine("Could not locate client" + client_name);
			}

			else
			{
				Console.WriteLine("Adding client: " + client_name + ", on port: " + port);
				_clients.Add(client_name, client);
			}
		}


		public void CloseMeeting(string client_name, string meeting_topic)
		{
			throw new NotImplementedException();
		}

		public void CreateMeeting(string owner_name, string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			throw new NotImplementedException();
		}

		public void JoinMeeting(string client_name, string meeting_topic, int slot_count, List<string> slots)
		{
			throw new NotImplementedException();
		}

	}
}

using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class Program
	{
		static void Main(string[] args)
		{
			Server server = new Server();
		}
	}


	public class Server : IServer
	{
		//key is meeting topic
		private Dictionary<string, Meeting> _meetings = new Dictionary<string, Meeting>();

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

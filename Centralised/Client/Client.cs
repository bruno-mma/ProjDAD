using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ClientForm());
		}
	}


	public class Client : IClient
	{
		private string _name;

		public Client(string name)
		{
			_name = name;
		}

		public void CloseMeeting(string meeting_topic)
		{
			throw new NotImplementedException();
		}

		public void CreateMeeting(string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			throw new NotImplementedException();
		}

		public void JoinMeeting(string meeting_topic)
		{
			throw new NotImplementedException();
		}

		public void ListMeetings()
		{
			throw new NotImplementedException();
		}
	}
}

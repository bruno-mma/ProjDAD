using Shared;
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

		private Dictionary<string, MeetingData> _knownMeetings = new Dictionary<string, MeetingData>();

		public Client(string name)
		{
			_name = name;
		}

		public void UpdateMeeting(string meeting_topic, Meeting meeting, MeetingData meetingData)
		{
			throw new NotImplementedException();
		}
	}
}
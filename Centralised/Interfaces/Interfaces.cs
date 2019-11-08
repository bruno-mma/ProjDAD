using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IServer
	{
		bool CreateMeeting(string owner_name, string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees);

		bool JoinMeeting(string client_name, string meeting_topic, int slot_count, List<string> slots);

		bool CloseMeeting(string client_name, string meeting_topic);

		bool AddClient(string client_name, int port);
	}

	public interface IClient
	{
		void UpdateMeeting(string meeting_topic, MeetingData meetingData);
	}

	public interface IMeeting
	{

	}
}

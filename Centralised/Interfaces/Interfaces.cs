using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IServer
	{
		void CreateMeeting(string owner_name, string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees);

		void JoinMeeting(string client_name, string meeting_topic, int slot_count, List<string> slots);

		void CloseMeeting(string client_name, string meeting_topic);
	}

	public interface IClient
	{
		void UpdateMeeting(string meeting_topic, IMeeting meeting, MeetingData meetingData);
	}

	public interface IMeeting
	{

	}
}

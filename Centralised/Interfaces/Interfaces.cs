using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IClient
	{
		//print to screen or update textbox
		void ListMeetings();

		void CreateMeeting(string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees);

		void JoinMeeting(string meeting_topic, int slot_count, List<string> slots);

		void CloseMeeting(string meeting_topic);
	}

	public interface IMeeting
	{

	}
}

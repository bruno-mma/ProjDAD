using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class Meeting : IMeeting
	{
		string _meetingTopic;

		int _minAttendees;
		int _numberOfSlots;
		int _numberOfInvitees;

		List<string> _slots;
		List<string> _invitees;

		bool _closed;

		//Subset of dates/location where a user is available, ex. “Maria, (2019-11-15, Porto)”
		List<string> _meetingRecords;

		public Meeting(string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			_meetingTopic = meeting_topic;
			_minAttendees = min_attendees;

			_numberOfSlots = number_of_slots;
			_numberOfInvitees = number_of_invitees;

			_slots = slots;
			_invitees = invitees;

			_closed = false;
			_meetingRecords = new List<string>();
		}
	}
}

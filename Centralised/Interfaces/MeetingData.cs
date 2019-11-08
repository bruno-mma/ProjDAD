using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
	[Serializable]
	public class MeetingData
	{
		public string _meetingTopic;

        public string _meetingOwner;

		public int _minAttendees;
		public int _numberOfSlots;
		public int _numberOfInvitees;

		
		//public List<string> _slots;	_meetingRecords already has information regarding the existing slots
		public List<string> _invitees;  //TODO: maybe change this into a hashtable for better performance

		public bool _closed;

		//Subset of dates/location where a user is available, ex. “Maria, (2019-11-15, Porto)”
		//key is slot (ex: "2019-11-15, Porto", each slot as a list of users that selected that slot (ex: <Maria, ...>);
		public Dictionary<string, List<string>> _meetingRecords;


		public MeetingData(string meeting_topic, string meetingOwner, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			_meetingTopic = meeting_topic;

            _meetingOwner = meetingOwner;

			_minAttendees = min_attendees;
			_numberOfSlots = number_of_slots;
			_numberOfInvitees = number_of_invitees;

			_invitees = invitees;

			_closed = false;
			_meetingRecords = new Dictionary<string, List<string>>();

			//create list for each slot, to save clients interested in that slot
			foreach (string slot in slots)
			{
				_meetingRecords.Add(slot, new List<string>());
			}
		}
	}
}

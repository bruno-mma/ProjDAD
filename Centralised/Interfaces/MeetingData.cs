﻿using System;
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

		public List<string> _slots;
		public List<string> _invitees;

		public bool _closed;

		//Subset of dates/location where a user is available, ex. “Maria, (2019-11-15, Porto)”
		public List<string> _meetingRecords;


		public MeetingData(string meeting_topic, string meetingOwner, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			_meetingTopic = meeting_topic;

            _meetingOwner = meetingOwner;

			_minAttendees = min_attendees;
			_numberOfSlots = number_of_slots;
			_numberOfInvitees = number_of_invitees;

			_slots = slots;
			_invitees = invitees;

			_closed = false;
			_meetingRecords = new List<string>();
		}

		public MeetingData()
		{
			//empty constructor is still needed
		}
	}
}

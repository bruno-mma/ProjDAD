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
		private MeetingData _meetingData;

		public string MeetingTopic { get => this._meetingData._meetingTopic; set => this._meetingData._meetingTopic = value; }

		public int MinAttendees { get => this._meetingData._minAttendees; set => this._meetingData._minAttendees = value; }
		public int NumberOfSlots { get => this._meetingData._numberOfSlots; set => this._meetingData._numberOfSlots = value; }
		public int NumberOfInvitees { get => this._meetingData._numberOfInvitees; set => this._meetingData._numberOfInvitees = value; }

		public List<string> Slots { get => this._meetingData._slots; set => this._meetingData._slots = value; }
		public List<string> Invitees { get => this._meetingData._invitees; set => this._meetingData._invitees = value; }

		public bool Closed { get => this._meetingData._closed; set => this._meetingData._closed = value; }

		public List<string> MeetingRecords { get => this._meetingData._meetingRecords; set => this._meetingData._meetingRecords = value; }


		public Meeting(string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			this._meetingData = new MeetingData();

			MeetingTopic = meeting_topic;
			MinAttendees = min_attendees;

			NumberOfSlots = number_of_slots;
			NumberOfInvitees = number_of_invitees;

			Slots = slots;
			Invitees = invitees;

			Closed = false;
			MeetingRecords = new List<string>();
		}
	}
}

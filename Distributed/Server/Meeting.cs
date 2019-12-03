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
        public MeetingData _meetingData;

        public string MeetingOwner { get => this._meetingData._meetingOwner; set => this._meetingData._meetingOwner = value; }

		public string MeetingTopic { get => this._meetingData._meetingTopic; set => this._meetingData._meetingTopic = value; }

		public int MinAttendees { get => this._meetingData._minAttendees; set => this._meetingData._minAttendees = value; }
		public int NumberOfSlots { get => this._meetingData._numberOfSlots; set => this._meetingData._numberOfSlots = value; }
		public int NumberOfInvitees { get => this._meetingData._numberOfInvitees; set => this._meetingData._numberOfInvitees = value; }

		//public List<string> Slots { get => this._meetingData._slots; set => this._meetingData._slots = value; }
		public List<string> Invitees { get => this._meetingData._invitees; set => this._meetingData._invitees = value; }

		public bool Closed { get => this._meetingData._closed; set => this._meetingData._closed = value; }

		public bool Canceled { get => this._meetingData._canceled; set => this._meetingData._canceled = value; }

		public Dictionary<string, List<string>> MeetingRecords { get => this._meetingData._meetingRecords; set => this._meetingData._meetingRecords = value; }


		public List<string> SelectedUsers { get => this._meetingData._selectedUsers; set => this._meetingData._selectedUsers = value; }

		public string SelectedRoom { get => this._meetingData._selectedRoom; set => this._meetingData._selectedRoom = value; }
		public string SelectedDate { get => this._meetingData._selectedDate; set => this._meetingData._selectedDate = value; }

		public Meeting(string meeting_topic, string meeting_owner, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			this._meetingData = new MeetingData(meeting_topic, meeting_owner, min_attendees, number_of_slots, number_of_invitees, slots, invitees);
		}
	}
}

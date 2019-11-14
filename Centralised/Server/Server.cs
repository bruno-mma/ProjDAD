﻿using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public class Program
	{
		public static readonly int _server_port = 8080;

		static void Main(string[] args)
		{
			TcpChannel channel = new TcpChannel(_server_port);
			ChannelServices.RegisterChannel(channel, false);

			Server server = new Server();

			//for now hard coded rooms
			server.AddRoom("Lisboa", "Room-A", 20);
			server.AddRoom("Lisboa", "Room-B", 10);
			server.AddRoom("Porto", "Room-C", 15);
			server.AddRoom("memeCity", "memeZone", 2);

			RemotingServices.Marshal(server, "Server", typeof(IServer));

			Console.ReadLine();
		}
	}

	public class Server : MarshalByRefObject, IServer
	{
		//key is meeting topic
		private Dictionary<string, Meeting> _meetings = new Dictionary<string, Meeting>();

		//key is client name
		private Dictionary<string, IClient> _clients = new Dictionary<string, IClient>();

		//occupied slots (with closed meetings), key is slot string, Ex: "Lisboa,2020-01-02"
		//private Dictionary<string, Meeting> _slots = new Dictionary<string, Meeting>();

		//Room locations, key is location name
		private Dictionary<string, Location> _locations = new Dictionary<string, Location>();

		public override object InitializeLifetimeService()
		{
			return null;
		}

		public bool AddClient(string client_name, int port)
		{
			IClient client = (IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:" + port + "/" + client_name);

			//weak check
			if (client == null)
			{
				Console.WriteLine("Could not locate client" + client_name);
				return false;
			}

			else
			{
				Console.WriteLine("Adding client: " + client_name + ", on port: " + port);
				_clients[client_name] = client;

				UpdateClientAllMeetings(client_name);
			}

			return true;
		}

		private void UpdateMeetingInvolvedClients(Meeting meeting)
		{
			if (meeting.NumberOfInvitees == 0)
			{
				foreach (var client in _clients)
				{
					client.Value.UpdateMeeting(meeting.MeetingTopic, meeting._meetingData);
					Console.WriteLine("Updated client " + client.Key + " with meeting " + meeting.MeetingTopic);
				}
			}
			else
			{
				foreach (string client_name in meeting.Invitees)
				{
					if (_clients.ContainsKey(client_name))
					{
						_clients[client_name].UpdateMeeting(meeting.MeetingTopic, meeting._meetingData);
						Console.WriteLine("Updated client " + client_name + " with meeting " + meeting.MeetingTopic);
					}
				}
			}
		}

		private void UpdateClientAllMeetings(string client_name)
		{
			foreach (Meeting meeting in _meetings.Values)
			{
				if (meeting.NumberOfInvitees == 0 || meeting.Invitees.Contains(client_name))
				{
					_clients[client_name].UpdateMeeting(meeting.MeetingTopic, meeting._meetingData);
					Console.WriteLine("Updated client " + client_name + " with meeting " + meeting.MeetingTopic);
				}
			}
		}


		public bool CloseMeeting(string client_name, string meeting_topic)
		{
			//if meeting does not exist, cant close it
			if (!_meetings.ContainsKey(meeting_topic))
			{
				return false;
			}

			Meeting meeting = _meetings[meeting_topic];

			//only meeting owner can close a meeting
			if (meeting.MeetingOwner != client_name)
			{
				return false;
			}

			List<Room> available_rooms = new List<Room>();
			List<string> available_dates = new List<string>();

			//for each of the possible locations, get available rooms
			foreach (KeyValuePair<string, List<string>> kvp in meeting.MeetingRecords)
			{
				List<string> interested_users = kvp.Value;

				// if this location does not have enough users interested, try the next one
				if (interested_users.Count < meeting.MinAttendees)
				{
					continue;
				}

				string location = kvp.Key.Split(',')[0];
				string date = kvp.Key.Split(',')[1];


				//find out if location has room available
				foreach (Room room in this._locations[location]._rooms.Values)
				{
					if (!room._dates.ContainsKey(date))
					{
						room.SetPotentialAttending(interested_users.Count);
						available_rooms.Add(room);
						available_dates.Add(date);
					}
				}
			}

			//if no room is available, cant close the meeting
			if (available_rooms.Count == 0)
			{
				//TODO: maybe cancel meeting?
				return false;
			}

			Room selected_room = available_rooms[0];
			string selected_date = available_dates[0];

			if (available_rooms.Count > 1)
			{
				//pick the room pick the room with the most potential attendees
				for (int i = 1; i < available_rooms.Count; i++)
				{
					if (Room.CompareRooms(selected_room, available_rooms[i]) == 1)
					{
						selected_room = available_rooms[i];
						selected_date = available_dates[i];
					}
				}
			}

			//schedule meeting
			meeting.Closed = true;
			meeting.SelectedDate = selected_date;
			meeting.SelectedRoom = selected_room._location + ',' + selected_room._name;

			string meeting_record = selected_room._location + ',' + selected_date;

			//add selected users to users attending meeting
			meeting.SelectedUsers = meeting.MeetingRecords[meeting_record].GetRange(0, selected_room.NumAvailable);

			//book room at selected date
			selected_room._dates.Add(selected_date, meeting.MeetingTopic);

			//update clients
			UpdateMeetingInvolvedClients(meeting);

			return true;
		}

		public bool CreateMeeting
			(string owner_name, string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			//meeting topic has to be unique
			if (_meetings.ContainsKey(meeting_topic))
			{
				return false;
			}

			bool valid_spot = false;

			//for a slot to be valid, the location has to exist
			for (int i = 0; i < slots.Count; i++)
			{
				string slot = slots[i];
				string location = slot.Split(',')[0];

				if (this._locations.ContainsKey(location))
				{
					valid_spot = true;
				}
				else
				{
					slots.RemoveAt(i--);
				}
			}

			//if no valid spot was given, meeting cant be created
			if (!valid_spot)
			{
				return false;
			}


			Meeting meeting = new Meeting(meeting_topic, owner_name, min_attendees, number_of_slots, number_of_invitees, slots, invitees);

			_meetings.Add(meeting_topic, meeting);

			string print = "Client " + owner_name + " created a meeting with topic: " + meeting_topic + ", with "
				+ min_attendees + " required atendees, with slots: ";

			foreach (string slot in slots)
			{
				print += slot + " ";
			}

			if (number_of_invitees < 0)
			{
				print += "and invitees: ";

				foreach (string invitee in invitees)
				{
					print += invitee + " ";
				}
			}

			Console.WriteLine(print);

			UpdateMeetingInvolvedClients(meeting);

			return true;
		}

		public bool JoinMeeting(string client_name, string meeting_topic, int slot_count, List<string> slots)
		{
			//if meeting does not exist, user cannot join
			if (!_meetings.ContainsKey(meeting_topic))
			{
				return false;
			}

			Meeting meeting = _meetings[meeting_topic];

			//if meeting is closed, user cannot join
			if (meeting.Closed)
			{
				return false;
			}

			//if meeting is invitees only, and user is not invited, user cannot join
			if (meeting.NumberOfInvitees > 0 && !meeting.Invitees.Contains(client_name))
			{
				return false;
			}

			bool joined = false;

			foreach (string slot in slots)
			{
				// add user into users interested in that slot, if slot does not exist, ignore that slot
				if (meeting.MeetingRecords.ContainsKey(slot) && !meeting.MeetingRecords[slot].Contains(client_name))
				{
					meeting.MeetingRecords[slot].Add(client_name);
					joined = true;
				}
			}

			if (joined)
			{
				// only update clients if meeting information was changed
				UpdateMeetingInvolvedClients(meeting);
			}

			return joined;
		}


		public void AddRoom(string location, string name, int capacity)
		{
			if (!_locations.ContainsKey(location))
			{
				_locations[location] = new Location(location);
			}

			_locations[location]._rooms.Add(name, new Room(location, name, capacity));
		}

	}
}
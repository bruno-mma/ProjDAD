using Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace Server
{
	public class Program
	{
		public static readonly int _server_port = 8080;

		static void Main(string[] args)
		{
			Server server;

			// non PCS execution
			if (args.Length == 1)
			{
				server = new Server();

				TcpChannel channel = new TcpChannel(_server_port);
				ChannelServices.RegisterChannel(channel, false);

				RemotingServices.Marshal(server, "Server", typeof(IServer));
			}

			// server started by PCS
			else
			{
				server = new Server(args[0], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]));

				int port = URL.GetPort(args[1]);
				string URI = URL.GetURI(args[1]);

				TcpChannel channel = new TcpChannel(port);
				ChannelServices.RegisterChannel(channel, false);

				RemotingServices.Marshal(server, URI, typeof(IServer));

				Console.WriteLine("Server running at " + args[1]);
			}

			Console.ReadLine();
		}
	}

	public class Server : MarshalByRefObject, IServer
	{
		string _id;

		int _maxFaults;
		int _minDelay;
		int _maxDelay;

		private Random _rdn = new Random();

		private bool _frozen = false;
		private List<Action> _messageBacklog = new List<Action>();

		//key is meeting topic
		private Dictionary<string, Meeting> _meetings = new Dictionary<string, Meeting>();

		//key is client name
		private Dictionary<string, IClient> _clients = new Dictionary<string, IClient>();

		//Room locations, key is location name
		private Dictionary<string, Location> _locations = new Dictionary<string, Location>();

		public Server(string server_id, int max_faults, int min_delay, int max_delay)
		{
			_id = server_id;

			_maxFaults = max_faults;
			_minDelay = min_delay;
			_maxDelay = max_delay;
		}

		public Server() : this("s1", 0, 0, 0)
		{
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		public bool IsFrozen()
		{
			return _frozen;
		}

		public void DelayMessage()
		{
			if (_maxDelay != 0 && _minDelay != 0)
			{
				Thread.Sleep(_rdn.Next(_minDelay, _maxDelay));
			}
		}

		public void Freeze ()
		{
			_frozen = true;

			Console.WriteLine("This server was frozen by the PuppetMaster");
		}

		public void Unfreeze ()
		{
			_frozen = false;

			Console.WriteLine("This server was unfrozen by the PuppetMaster");

			foreach (Delegate message in _messageBacklog)
			{
				message.DynamicInvoke();
			}

			_messageBacklog = new List<Action>();
		}

		public bool AddClient(string client_URL, string client_name)
		{
			if (_frozen)
			{
				_messageBacklog.Add(() => AddClient(client_URL, client_name));
			}

			IClient client = (IClient)Activator.GetObject(typeof(IClient), client_URL);

			//weak check
			if (client == null)
			{
				Console.WriteLine("Could not locate client at" + client_URL);
				return false;
			}

			else
			{
				Console.WriteLine("Adding client at: " + client_URL);
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
					//Console.WriteLine("Updated client " + client.Key + " with meeting " + meeting.MeetingTopic);
				}
			}
			else
			{
				foreach (string client_name in meeting.Invitees)
				{
					if (_clients.ContainsKey(client_name))
					{
						_clients[client_name].UpdateMeeting(meeting.MeetingTopic, meeting._meetingData);
						//Console.WriteLine("Updated client " + client_name + " with meeting " + meeting.MeetingTopic);
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
					//Console.WriteLine("Updated client " + client_name + " with meeting " + meeting.MeetingTopic);
				}
			}
		}


		public string CloseMeeting(string client_name, string meeting_topic)
		{
			if (_frozen)
			{
				_messageBacklog.Add(() => CloseMeeting(client_name, meeting_topic));
			}

			//if meeting does not exist, cant close it
			if (!_meetings.ContainsKey(meeting_topic))
			{
				return "Error: Meeting " + meeting_topic + " does not exist";
			}

			Meeting meeting = _meetings[meeting_topic];

			//if meeting was canceled, cant close it
			if (meeting.Canceled)
			{
				return "Error: Meeting " + meeting_topic + " was canceled, cannot close meeting";
			}

			//if meeting is already closed, cant close again
			if (meeting.Closed)
			{
				return "Error: Meeting " + meeting_topic + " already closed";
			}

			//only meeting owner can close a meeting
			if (meeting.MeetingOwner != client_name)
			{
				return "Error: Only meeting owner (" + meeting.MeetingOwner + ") can close meeting " + meeting_topic;
			}



			bool minimum_attending = false;

			// check if there is at least one location with sufficient interested users
			foreach (KeyValuePair<string, List<string>> kvp in meeting.MeetingRecords)
			{
				List<string> interested_users = kvp.Value;

				if (interested_users.Count >= meeting.MinAttendees)
				{
					minimum_attending = true;
					break;
				}
			}

			if (!minimum_attending)
			{
				return "Error: No location has the minimum number of interested users, cannot close meeting";
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
				meeting.Canceled = true;
				return "Error: no room available at selected dates, canceling meeting";
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

			string client_print_message = "Successfully closed meeting " + meeting_topic + " at room " + meeting.SelectedRoom + " at date " + meeting.SelectedDate + ", with users:";
			client_print_message += Environment.NewLine;

			foreach (string user in meeting.SelectedUsers)
			{
				client_print_message += user + " ";
			}

			return client_print_message;
		}

		public string CreateMeeting
			(string owner_name, string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
			if (_frozen)
			{
				_messageBacklog.Add(() => CreateMeeting(owner_name, meeting_topic, min_attendees, number_of_slots, number_of_invitees, slots, invitees));
			}

			//meeting topic has to be unique
			if (_meetings.ContainsKey(meeting_topic))
			{
				return "Error: meeting with topic " + meeting_topic + " already exists, cannot create meeting";
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
				return "Error: no valid location was given, cannot create meeting";
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


			string client_print_message = "Created a meeting with topic: " + meeting_topic + ", with " + min_attendees + " required atendees, with slots: ";

			foreach (string slot in slots)
			{
				client_print_message += slot + " ";
			}

			if (number_of_invitees < 0)
			{
				client_print_message += "and invitees: ";

				foreach (string invitee in invitees)
				{
					client_print_message += invitee + " ";
				}
			}

			return client_print_message;
		}

		public string JoinMeeting(string client_name, string meeting_topic, int slot_count, List<string> slots)
		{
			if (_frozen)
			{
				_messageBacklog.Add(() => JoinMeeting(client_name, meeting_topic, slot_count, slots));
			}

			//if meeting does not exist, user cannot join
			if (!_meetings.ContainsKey(meeting_topic))
			{
				return "Error: meeting " + meeting_topic + " does not exist, cannot join";
			}

			Meeting meeting = _meetings[meeting_topic];

			//if meeting is closed, user cannot join
			if (meeting.Closed)
			{
				return "Error: meeting " + meeting_topic + " is closed, cannot join";
			}

			//if meeting is invitees only, and user is not invited, user cannot join
			if (meeting.NumberOfInvitees > 0 && !meeting.Invitees.Contains(client_name))
			{
				return "Error: user is not invited to this meeting, cannot join";
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

			return "Joined meeting with topic: " + meeting_topic;
		}

		public void SetRooms(Dictionary<string, Location> locations)
		{
			_locations = locations;
		}
	}
}
using Interfaces;
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
		private Dictionary<string, Meeting> _slots = new Dictionary<string, Meeting>();


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
				_clients.Add(client_name, client);
			}

			return true;
		}
		private void UpdateMeetingInvolvedClients(Meeting meeting)
		{
			if (meeting.NumberOfInvitees == 0)
			{
				foreach (IClient client in _clients.Values)
				{
					client.UpdateMeeting(meeting.MeetingTopic, meeting._meetingData);
				}
			}
			else
			{
				foreach (string client_name in meeting.Invitees)
				{
					if (_clients.ContainsKey(client_name))
					{
						_clients[client_name].UpdateMeeting(meeting.MeetingTopic, meeting._meetingData);
					}
				}
			}
		}


		public bool CloseMeeting(string client_name, string meeting_topic)
		{
			throw new NotImplementedException();
		}

		public bool CreateMeeting(string owner_name, string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
            Meeting meeting = new Meeting(meeting_topic, owner_name, min_attendees, number_of_slots, number_of_invitees, slots, invitees);

            _meetings.Add(meeting_topic, meeting);

			string print = "Client " + owner_name + " created a meeting with topic: " + meeting_topic + ", with " + min_attendees + " required atendees, with slots: ";

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
				if (meeting.MeetingRecords.ContainsKey(slot))
				{
					meeting.MeetingRecords[slot].Add(client_name);
					joined = true;
				}
				else
				{
					slots.Remove(slot);
				}
			}

			if (joined)
			{
				// only update clients if meeting information was changed
				UpdateMeetingInvolvedClients(meeting);
			}

			return joined;
		}

	}
}

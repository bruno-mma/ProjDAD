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


		public void AddClient(string client_name, int port)
		{
			IClient client = (IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:" + port + "/" + client_name);

			//weak check
			if (client == null)
			{
				Console.WriteLine("Could not locate client" + client_name);
			}

			else
			{
				Console.WriteLine("Adding client: " + client_name + ", on port: " + port);
				_clients.Add(client_name, client);
			}
		}


		public void CloseMeeting(string client_name, string meeting_topic)
		{
			throw new NotImplementedException();
		}

		public void CreateMeeting(string owner_name, string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees)
		{
            Meeting meeting = new Meeting(meeting_topic, owner_name, min_attendees, number_of_slots, number_of_invitees, slots, invitees);
            //MeetingData meetingData = new MeetingData(meeting_topic, owner_name, min_attendees, number_of_slots, number_of_invitees, slots, invitees);
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

			// supposing number_of_invitees is 0 if there are no invitees (invitees list is empty)
			// TODO: move code to function that sends to all involved clients
			if (number_of_invitees == 0)
            {
                foreach (var client in _clients)
                {
                    client.Value.UpdateMeeting(meeting.MeetingTopic, meeting._meetingData);
                }
            }

            else
            {
                foreach (var client in invitees)
                {
                    if (_clients.ContainsKey(client))
                    {
                        _clients[client].UpdateMeeting(meeting.MeetingTopic, meeting._meetingData);
					}
                }
            }
		}

		public void JoinMeeting(string client_name, string meeting_topic, int slot_count, List<string> slots)
		{
			throw new NotImplementedException();
		}
	}
}

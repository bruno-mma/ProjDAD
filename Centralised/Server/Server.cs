using Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Server
{
	public class Program
	{
		public static readonly int _server_port = 8080;

		public static object RemoteChannelProperties { get; private set; }

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
				
				if (args.Length == 5)
					server = new Server(args[0], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]), args[1]);
				else
					server = new Server(args[0], Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4]), args[5], args[1]);


				int port = URL.GetPort(args[1]);
				string URI = URL.GetURI(args[1]);


				//TcpChannel channel = new TcpChannel(port);
				IDictionary RemoteChannelProperties = new Hashtable();
				RemoteChannelProperties["port"] = port;
				RemoteChannelProperties["name"] = args[0];
				TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, null);

				ChannelServices.RegisterChannel(channel, false);
				RemotingServices.Marshal(server, URI, typeof(IServer));

				Console.WriteLine("Server running at " + args[1]);

				server.AddServerURLToFile(args[0], args[1]);
			}

			Console.ReadLine();
		}
	}

	public class Server : MarshalByRefObject, IServer
	{
		/*---------------------------------------------------------------------------------------------------------------------
		 * ------------------------------------------------------Declarations--------------------------------------------------
		 * --------------------------------------------------------------------------------------------------------------------*/
			


		string _id;
		string _localURL;
		int _maxFaults;
		int _minDelay;
		int _maxDelay;

		static int _msgReceived =0;



		//testing
		public delegate string RemoteAsyncDelegate(string joinId, string client_name, string meeting_topic, int slot_count, List<string> slots);
		//key is "join" + _joinId++
		public int _joinId = 0;
		private static Dictionary<string, int> _joinReplicationCounter = new Dictionary<string, int>();
		private static Mutex mut = new Mutex();
		private static Mutex mut2 = new Mutex();



		private Random _rdn = new Random();

		private bool _frozen = false;
		private List<Action> _messageBacklog = new List<Action>();

		//key is meeting topic
		private Dictionary<string, Meeting> _meetings = new Dictionary<string, Meeting>();

		//key is client name
		private Dictionary<string, IClient> _clients = new Dictionary<string, IClient>();

		//key is server id
		private Dictionary<string, IServer> _servers = new Dictionary<string, IServer>();

		//Room locations, key is location name
		private Dictionary<string, Location> _locations = new Dictionary<string, Location>();

		private readonly string serverURLsPath = @"..\..\..\" + "serverURLs.txt";


		/*---------------------------------------------------------------------------------------------------------------------
		 * ------------------------------------------------------Constructors--------------------------------------------------
		 * --------------------------------------------------------------------------------------------------------------------*/



		public Server(string server_id, int max_faults, int min_delay, int max_delay, string lURL)
		{
			_id = server_id;
			_localURL= lURL;
			_maxFaults = max_faults;
			_minDelay = min_delay;
			_maxDelay = max_delay;

		}

		public Server(string server_id, int max_faults, int min_delay, int max_delay, string allServersURL, string lURL)
		{
			_id = server_id;
			_localURL = lURL;
			_maxFaults = max_faults;
			_minDelay = min_delay;
			_maxDelay = max_delay;

			string[] KeyPars = allServersURL.Split(';');
			foreach (string keyPar in KeyPars)
			{
				if (!string.IsNullOrEmpty(keyPar))
				{
					string[] KeyPar = keyPar.Split(',');
					try
					{
						IServer server = (IServer)Activator.GetObject(typeof(IServer), KeyPar[1]);
						Console.WriteLine("tcp://" + GetLocalIPAddress() + ":" + URL.GetPort(_localURL) + "/" + URL.GetURI(_localURL));
						server.AddServer(server_id, "tcp://" + GetLocalIPAddress() + ":"+URL.GetPort(_localURL)+"/"+ URL.GetURI(_localURL));
						_servers.Add(KeyPar[0], server);
						Console.WriteLine("Try success");
					}
					catch (Exception)
					{
						Console.WriteLine("The server " + KeyPar[0] + " was unavailable");
					}
				}

			}
			Console.WriteLine("size of _servers: " + _servers.Count);
			
		}

		public Server() : this("s1", 0, 0, 0, "8080")
		{
		}

		/*---------------------------------------------------------------------------------------------------------------------
		 * ------------------------------------------------------Main Functions------------------------------------------------
		 * --------------------------------------------------------------------------------------------------------------------*/


		public string CloseMeeting(string client_name, string meeting_topic)
		{
			DelayMessage();

			if (_frozen)
			{
				_messageBacklog.Add(() => CloseMeeting(client_name, meeting_topic));

				return "";
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
			DelayMessage();

			if (_frozen)
			{
				_messageBacklog.Add(() => CreateMeeting(owner_name, meeting_topic, min_attendees, number_of_slots, number_of_invitees, slots, invitees));

				return "";
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
			DelayMessage();

			if (_frozen)
			{
				_messageBacklog.Add(() => JoinMeeting(client_name, meeting_topic, slot_count, slots));

				return "";
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

			//Testing 2
			Console.WriteLine("entra sleep");
			Thread.Sleep(12000);
			Console.WriteLine("sai sleep");

			//Console.WriteLine("1joinId: " + _joinId, 0);
			mut.WaitOne();
			int thisJoinID =++_joinId;
			mut.ReleaseMutex();
			//Console.WriteLine("2joinId: " + _joinId, 0);
			_joinReplicationCounter.Add(_id+"_join_" + thisJoinID+"_"+, 0);

			Console.WriteLine("thisJoinID: " + thisJoinID);


			AsyncCallback RemoteCallback = new AsyncCallback(Server.OurRemoteAsyncCallBack);

			foreach (KeyValuePair<string, IServer> server in _servers)
			{
				// Create delegate to remote method
				Console.WriteLine("envia para: "+server.Key);
				RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate((server.Value).ReplicateJoin);
				// Call delegate to remote method
				IAsyncResult RemAr = RemoteDel.BeginInvoke("_join_"+thisJoinID, client_name, meeting_topic, slot_count, slots, RemoteCallback, null);
				Console.WriteLine("ja enviou para: " + server.Key);
			}
			Console.WriteLine("sucess1");
			while (_joinReplicationCounter["_join_"+ thisJoinID] != 2)
			{
				Thread.Sleep(500);
				Console.WriteLine("waiting...");
			}
			Console.WriteLine("sucess2");

			//Testing
			// Create delegate to remote method
			/*if (_id == "s1")
			{
				Console.WriteLine("entra sleep");
				Thread.Sleep(10000);
				Console.WriteLine("sai sleep");
				RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(_servers["s2"].MsgBack);
				// Call delegate to remote method
				IAsyncResult RemAr = RemoteDel.BeginInvoke("join"+thisJoinID,null, null);
				// Wait for the end of the call and then explictly call EndInvoke
				int count = 0;
				while (count != 3)
				{
					Console.WriteLine("msg received father: " + _msgReceived);
					Thread.Sleep(1000);
					count++;
				}
				RemAr.AsyncWaitHandle.WaitOne();
				Console.WriteLine("sucess1");
				//Console.WriteLine(RemoteDel.EndInvoke(RemAr));
				RemoteDel.EndInvoke(RemAr);
				Console.WriteLine("sucess2");
			}*/

			/*Server server=new Server();
			Thread thread = new Thread(server.MsgBack);
			thread.Start();
			Console.WriteLine("first thread begun");
			
			Server server2 = new Server();
			Thread thread2 = new Thread(server.MsgBack);
			thread2.Start();
			Console.WriteLine("second thread begun");

			Server server3 = new Server();
			Thread thread3 = new Thread(server.MsgBack);
			thread3.Start();
			Console.WriteLine("third thread begun");*/



			return "Joined meeting with topic: " + meeting_topic;
		}

		public string ReplicateJoin(string joinId, string client_name, string meeting_topic, int slot_count, List<string> slots)
		{

			Thread.Sleep(10000);
			JoinMeeting(client_name, meeting_topic, slot_count, slots);
			Console.WriteLine("thread"+ joinId);
			return joinId;
		}

		public static void OurRemoteAsyncCallBack(IAsyncResult ar)
		{
			RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
			string a = del.EndInvoke(ar);
			Console.WriteLine("Received: " + a);
			//mut2.WaitOne();
			_joinReplicationCounter[a]++;
			//mut2.ReleaseMutex();
			Console.WriteLine("_joinReplicationCounter[del.EndInvoke(ar)]: " + _joinReplicationCounter[del.EndInvoke(ar)]);
			return;
		}






		/*---------------------------------------------------------------------------------------------------------------------
		 * ---------------------------------------------------------Functions--------------------------------------------------
		 * --------------------------------------------------------------------------------------------------------------------*/


		public void AddServerURLToFile(string server_id, string server_URL)
		{
			using (StreamWriter sw = File.AppendText(serverURLsPath))
			{
				sw.WriteLine(server_id + ' ' + server_URL);
			}
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

		public void Freeze()
		{
			_frozen = true;

			Console.WriteLine("This server was frozen by the PuppetMaster");
		}

		public void Unfreeze()
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
			DelayMessage();

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
			   
		

		public void SetRooms(Dictionary<string, Location> locations)
		{
			_locations = locations;
		}

		public void Crash()
		{
			RemotingServices.Disconnect(this);
			Environment.Exit(0);
		}


		public void AddServer(string serverId, string serverURL)
		{
			_servers.Add(serverId, (IServer)Activator.GetObject(typeof(IServer), serverURL));
			Console.WriteLine("size of _servers: " + _servers.Count);
		}

		public static string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
}
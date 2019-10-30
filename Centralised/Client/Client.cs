﻿using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ClientForm());
		}
	}


	public class Client : MarshalByRefObject, IClient
	{
		private readonly static int _initial_port = 9000;

		private readonly ClientForm _form;
		private string _name;
		private IServer _server;

		private Dictionary<string, MeetingData> _knownMeetings = new Dictionary<string, MeetingData>();

		public Client(ClientForm form)
		{
			_form = form;
			_form._client = this;
		}

		public void Connect(string name)
		{
			//for now
			int port = _initial_port;
			bool open_port_found = false;

			while(!open_port_found)
			{
				try
				{
					TcpChannel channel = new TcpChannel(port);
					ChannelServices.RegisterChannel(channel, false);
					open_port_found = true;
				}
				catch (System.Net.Sockets.SocketException)
				{
					port++;
				}
			}


			//get remote server object
			IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://localhost:8080/Server");

			this._name = name;

			//weak check
			if (server == null)
			{
				Console.WriteLine("Could not locate server");
				return;
			}

			_server = server;

			//publish remote client object
			RemotingServices.Marshal(this, name, typeof(IClient));

			server.AddClient(name, port);
		}

		public void UpdateMeeting(string meeting_topic, IMeeting meeting, MeetingData meetingData)
		{
			throw new NotImplementedException();
		}
	}
}
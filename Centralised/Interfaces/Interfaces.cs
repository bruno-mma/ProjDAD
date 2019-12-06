using System.Collections.Generic;

namespace Interfaces
{
	public interface IServer
	{
		string CreateMeeting(string owner_name, string meeting_topic, int min_attendees, int number_of_slots, int number_of_invitees, List<string> slots, List<string> invitees);

		string JoinMeeting(string client_name, string meeting_topic, int slot_count, List<string> slots);

		string CloseMeeting(string client_name, string meeting_topic);

		bool AddClient(string client_URL, string client_name);

		void SetRooms(Dictionary<string, Location> locations);

        void Freeze();

        void Unfreeze();

		bool IsFrozen();

        void Crash();

		void AddServer(string serverId, string serverURL);

		string ReplicateJoin(string joinId, string client_name, string meeting_topic, int slot_count, List<string> slots);
	}

	public interface IClient
	{
		void UpdateMeeting(string meeting_topic, MeetingData meetingData);
	}

	public interface IMeeting
	{

	}

	public interface IPCS
	{
		bool LifeCheck();
		void StartServer(string server_id, string URL, int max_faults, int min_delay, int max_delay, string allServersURL);
		void StartServer(string server_id, string URL, int max_faults, int min_delay, int max_delay);		
		void StartClient(string name, string user_URL, string server_URL, string script_file);
	}
}

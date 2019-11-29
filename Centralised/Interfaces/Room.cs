using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
	[Serializable]
	public class Room
	{
		public string _location;
		public string _name;

		public int _capacity;

		//booked dates (with closed meeting), date -> meeting topic, ex: "2019-11-14" -> "meme talk"
		public Dictionary<string, string> _dates = new Dictionary<string, string>();

		//used for determining the most desirable room for a meeting
		public int NumAvailable { get; private set; }

		public Room(string location, string room_name, int capacity)
		{
			_location = location;
			_name = room_name;

			_capacity = capacity;
		}

		public static int CompareRooms(Room r1, Room r2)
		{
			//if both have the same number of potential attending users
			if (r1.NumAvailable == r2.NumAvailable)
			{
				//pick the one with the lowest capacity
				if (r1._capacity > r2._capacity) return 1;
				if (r1._capacity < r2._capacity) return -1;

				return 0;
			}

			//pick the one with the highest number of potential attending users
			if (r1.NumAvailable > r2.NumAvailable) return -1;
			if (r1.NumAvailable < r2.NumAvailable) return 1;

			return 0;
		}

		public void SetPotentialAttending(int interested)
		{
			if (interested > _capacity)
			{
				NumAvailable = _capacity;
			}

			else
			{
				NumAvailable = interested;
			}
		}
	}
}

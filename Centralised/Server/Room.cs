using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class Room
	{
		public string _location;
		public string _name;

		public int _capacity;

		//booked dates, date -> meeting topic, ex: "2019-11-14" -> "meme talk"
		public Dictionary<string, string> _dates = new Dictionary<string, string>();

		public Room(string location, string room_name, int capacity)
		{
			_location = location;
			_name = room_name;

			_capacity = capacity;
		}
	}
}

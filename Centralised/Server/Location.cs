using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class Location
	{
		public string _name;

		//key is room name
		public Dictionary<string, Room> _rooms = new Dictionary<string, Room>();

		public Location(string name)
		{
			_name = name;
		}
	}
}

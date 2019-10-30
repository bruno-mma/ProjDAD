using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
	public partial class ClientForm : Form
	{
		public Client _client;
		public ClientForm()
		{
			InitializeComponent();
		}

		private void ConnectButton_Click(object sender, EventArgs e)
		{
			if (NameTextBox.Text != "")
			{
				_client = new Client(this);

				_client.Connect(NameTextBox.Text);
			}
		}
	}
}

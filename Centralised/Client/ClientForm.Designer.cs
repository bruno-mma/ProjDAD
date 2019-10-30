namespace Client
{
	partial class ClientForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.NameTextBox = new System.Windows.Forms.TextBox();
			this.ConnectButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// NameTextBox
			// 
			this.NameTextBox.Location = new System.Drawing.Point(12, 12);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = new System.Drawing.Size(164, 20);
			this.NameTextBox.TabIndex = 0;
			// 
			// ConnectButton
			// 
			this.ConnectButton.Location = new System.Drawing.Point(195, 12);
			this.ConnectButton.Name = "ConnectButton";
			this.ConnectButton.Size = new System.Drawing.Size(141, 20);
			this.ConnectButton.TabIndex = 1;
			this.ConnectButton.Text = "Connect";
			this.ConnectButton.UseVisualStyleBackColor = true;
			this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
			// 
			// ClientForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(348, 450);
			this.Controls.Add(this.ConnectButton);
			this.Controls.Add(this.NameTextBox);
			this.Name = "ClientForm";
			this.Text = "Calendar";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox NameTextBox;
		private System.Windows.Forms.Button ConnectButton;
	}
}


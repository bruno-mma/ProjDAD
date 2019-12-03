using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PuppetMaster
{
	class PuppetMasterParser
	{
		private PuppetMaster _puppetMaster;

		public PuppetMasterParser(PuppetMaster puppet_master)
		{
			_puppetMaster = puppet_master;
		}

		private void PrintErrorMessage(string command)
		{
			Console.WriteLine("PuppetMasterParser, Error parsing command " + command);
		}

		//check if command has a minimum number of arguments
		private bool WrongArgumentCount(List<string> arguments, int correctCount)
		{
			if (arguments.Count - 1 < correctCount)
			{
				PrintErrorMessage(arguments[0]);
				return true;
			}

			return false;
		}


		public void ParseExecute(string command)
		{
			List<string> arguments = new List<string>(command.Split(' '));

			//remove empty strings from argument list
			arguments = arguments.Where(argument => !string.IsNullOrWhiteSpace(argument)).ToList();

			//ignore empty lines
			if (arguments.Count == 0)
			{
				return;
			}


			switch (arguments[0].ToLower())
			{
				case "quit":
				case "exit":
					Environment.Exit(0);
					break;

				// Async
				case "client":
					StartClient(arguments);
					break;

				// Async
				case "server":
					StartServer(arguments);
					break;

				case "addroom":
					AddRoom(arguments);
					break;

				case "addpcs":
					AddPCS(arguments);
					break;

				case "wait":
					Wait(arguments);
					break;

				// Async
				case "freeze":
					Freeze(arguments);
					break;

				// Async
				case "unfreeze":
					Unfreeze(arguments);
					break;

				case "run":
					RunScript(arguments, false);
					break;

				case "runp":
					RunScript(arguments, true);
					break;

				case "status":
					Status();
					break;

				case "crash":
					CrashServer(arguments);
					break;
					

				default:
					Console.WriteLine("Error: " + arguments[0] + " command not found");
					break;
			}
		}

		private void StartClient(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 4)) return;

			//_puppetMaster.StartClient(arguments[1], arguments[2], arguments[3], arguments[4]);

			// Create delegate to remote method
			PuppetMaster.RemoteAsyncStartClientDelegate RemoteDel = new PuppetMaster.RemoteAsyncStartClientDelegate(_puppetMaster.StartClient);

			// Dont need invocation result so no callback function

			// Call remote method
			RemoteDel.BeginInvoke(arguments[1], arguments[2], arguments[3], arguments[4], null, null);
		}

		private void StartServer(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 5)) return;

			//_puppetMaster.StartServer(arguments[1], arguments[2], Int32.Parse(arguments[3]), Int32.Parse(arguments[4]), Int32.Parse(arguments[5]));

			// Create delegate to remote method
			PuppetMaster.RemoteAsyncStartServerDelegate RemoteDel = new PuppetMaster.RemoteAsyncStartServerDelegate(_puppetMaster.StartServer);

			// Create delegate to local callback
			AsyncCallback RemoteCallback = new AsyncCallback(PuppetMaster.StartServerCallBack);

			// Call remote method
			RemoteDel.BeginInvoke(arguments[1], arguments[2], Int32.Parse(arguments[3]), Int32.Parse(arguments[4]), Int32.Parse(arguments[5]), RemoteCallback, null);
		}

		private void AddRoom(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 3)) return;

			_puppetMaster.AddRoom(arguments[1], arguments[3], Int32.Parse(arguments[2]));
		}

		private void Wait(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			Thread.Sleep(Int32.Parse(arguments[1]));
		}

		private void AddPCS(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			_puppetMaster.AddPCS(arguments[1]);
		}

		private void Freeze(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			//_puppetMaster.FreezeServer(arguments[1]);

			// Create delegate to remote method
			PuppetMaster.RemoteAsyncFreezeDelegate RemoteDel = new PuppetMaster.RemoteAsyncFreezeDelegate(_puppetMaster.FreezeServer);

			// Create delegate to local callback
			AsyncCallback RemoteCallback = new AsyncCallback(PuppetMaster.FreezeCallBack);

			// Call remote method
			RemoteDel.BeginInvoke(arguments[1], RemoteCallback, null);
		}

		private void Unfreeze(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			//_puppetMaster.UnfreezeServer(arguments[1]);

			// Create delegate to remote method
			PuppetMaster.RemoteAsyncFreezeDelegate RemoteDel = new PuppetMaster.RemoteAsyncFreezeDelegate(_puppetMaster.UnfreezeServer);

			// Create delegate to local callback
			AsyncCallback RemoteCallback = new AsyncCallback(PuppetMaster.FreezeCallBack);

			// Call remote method
			RemoteDel.BeginInvoke(arguments[1], RemoteCallback, null);
		}

		private void Status()
		{
			//_puppetMaster.PrintStatus();

			// Create delegate to remote method
			PuppetMaster.RemoteAsyncStatusDelegate RemoteDel = new PuppetMaster.RemoteAsyncStatusDelegate(_puppetMaster.PrintStatus);

			// Create delegate to local callback
			AsyncCallback RemoteCallback = new AsyncCallback(PuppetMaster.StatusCallBack);

			// Call remote method
			RemoteDel.BeginInvoke(RemoteCallback, null);
		}

		private void RunScript(List<string> arguments, bool pause)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			try
			{
				//assuming script is inside the solution folder
				string[] lines = System.IO.File.ReadAllLines(@"..\..\..\" + arguments[1] + ".txt");

				foreach (string line in lines)
				{
					if (pause)
					{
						Console.WriteLine("Press enter to run Command:" + line);
						Console.ReadLine();
					}
					else
					{
						Console.WriteLine("Running Command: " + line);
					}

					this.ParseExecute(line);
					Console.WriteLine();
				}

				Console.Write("Finished script execution.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.Write("PuppetMasterParser: " +  arguments[1] + ".txt File not found.");
			}

			Console.WriteLine(" Reading commands from console");
		}
	
		private void Status()
		{
			_puppetMaster.PrintStatus();
		}

		private void CrashServer(List<string> arguments)
		{
			if (WrongArgumentCount(arguments, 1)) return;

			_puppetMaster.CrashServer(arguments[1]);
		}
	}
}


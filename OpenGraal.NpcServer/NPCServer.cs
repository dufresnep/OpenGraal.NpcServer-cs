using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.NpcServer;
using OpenGraal.Common;
using OpenGraal.Common.Levels;
using OpenGraal.Common.Scripting;
using OpenGraal.Common.Players;

namespace OpenGraal.NpcServer
{
	public class Framework //: IFramework
	{
		#region Member variables
		/// <summary>
		/// Member Variables
		/// </summary>
		protected AsyncCallback cNCAccept;
		internal GameCompiler Compiler;
		public GServerConnection GSConn;
		protected TcpListener NCListen;
		private int milliseconds = 0;
		protected int TimerId;
		//protected TimerEventHandler TimerHandle;
		internal object TimerLock = new object ();
 
		// NC Variables
		public int NWTime = 0;
		public List<NCConnection> NCList = new List<NCConnection> ();
		public String NCMsg;

		// Dictionarys
		public Dictionary<int, ScriptObj> ScriptList = new Dictionary<int, ScriptObj> ();
		public Dictionary<String, GraalLevel> LevelList = new Dictionary<String, GraalLevel> ();
		public Dictionary<String, ServerClass> ClassList = new Dictionary<String, ServerClass> ();
		public Dictionary<String, ServerWeapon> WeaponList = new Dictionary<String, ServerWeapon> ();
		public Players.PlayerList PlayerManager = null;
		public bool running = true;
		#endregion

		#region Constructor / Destructor
		/// <summary>
		/// Creates a new NPCServer
		/// </summary>
		internal Framework (string OptionsFile = "settings.ini")
		{
			// Default PM
			this.NCMsg = CString.tokenize ("I am the npcserver for\nthis game server. Almost\nall npc actions are controled\nby me.");

			// Create Compiler
			Compiler = new GameCompiler (this);

			// Create Player Manager
			PlayerManager = new Players.PlayerList (this, GSConn);
			AppSettings settings = AppSettings.GetInstance ();
			settings.Load (OptionsFile);

			// Connect to GServer
			GSConn = new GServerConnection (this);

			this.ConnectToGServer();

			// Setup NPC-Control Listener
			cNCAccept = new AsyncCallback (NCControl_Accept);
			this.UPnPOpenPort();
			NCListen = new TcpListener (IPAddress.Parse (this.LocalIPAddress ()), settings.NCPort);
			NCListen.Start ();
			NCListen.BeginAcceptSocket (cNCAccept, NCListen);

			settings.Save ();
			
			// Setup Timer
			//timeBeginPeriod(50);
			//TimerHandle = new TimerEventHandler(RunServer);
			//TimerId = timeSetEvent(50, 0, TimerHandle, IntPtr.Zero, EVENT_TYPE);
		}

		private void ConnectToGServer()
		{
			AppSettings settings = AppSettings.GetInstance();
			if (!GSConn.Connected)
			{
				GSConn.Connect(settings.Address, settings.Port);
				if (GSConn.Connected)
				{
					GSConn.SendLogin(settings.Account, settings.Password, settings.Nickname);
					GSConn.ReceiveData();
				}
			}
		}

		private void UPnPOpenPort()
		{
			Console.WriteLine("[UPnP]: Sending port forwarding data!");
			AppSettings settings = AppSettings.GetInstance();
			NATUPNPLib.IUPnPNAT upnpnat = new NATUPNPLib.UPnPNAT();
			NATUPNPLib.IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;
			bool mapped = false;
			try
			{
				foreach (NATUPNPLib.IStaticPortMapping mapping in mappings)
				{
					//Console.WriteLine("[UPnP]: Mapping -> ip: "+mapping.InternalClient.ToString()+" port: " + mapping.InternalPort.ToString());
					if (this.LocalIPAddress().ToString() == mapping.InternalClient.ToString() && mapping.InternalPort.ToString() == settings.NCPort.ToString())
						mapped = true;
				}
			}
			catch (Exception e)
			{
			}

			if (!mapped)
			{
				try
				{
					mappings.Remove(settings.NCPort, "TCP");
				}
				catch (Exception e)
				{ }

				try
				{
					mappings.Add(settings.NCPort, "TCP", settings.NCPort, this.LocalIPAddress(), true, "OpenGraal NPCServer");
				}
				catch (Exception e)
				{ }
			}
		}

		/// <summary>
		/// Detructor -> Cleanup
		/// </summary>
		~Framework ()
		{
			this.running = false;
			//timeKillEvent(this.TimerId);
			//timeEndPeriod(50);
			
		}
		#endregion
		public string LocalIPAddress ()
		{
			IPHostEntry host;
			string localIP = "";
			host = Dns.GetHostEntry (Dns.GetHostName ());
			foreach (IPAddress ip in host.AddressList) {
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					localIP = ip.ToString ();
					break;
				}
			}
			return localIP;
		}
		/// <summary>
		/// Accept incoming connection for NPC-Control
		/// </summary>
		/// <param name="ar"></param>
		protected void NCControl_Accept (IAsyncResult ar)
		{
			// Accept Socket
			NCConnection Client = new NCConnection (this, (Socket)NCListen.EndAcceptSocket (ar));
			Client.ReceiveData ();
			NCList.Add (Client);

			// Listen for incoming connections
			NCListen.BeginAcceptSocket (cNCAccept, NCListen);
		}	

		#region Public functions
		/// <summary>
		/// Run NPC Server
		/// </summary>
		public bool RunServer ()
		{
			this.milliseconds +=50;
			
			// Reconnect to the GServer if connection was lost.
			this.ConnectToGServer();

			// Run Timeouts
			foreach (KeyValuePair<String, ServerWeapon> wep in WeaponList) {
				//
				ScriptObj obj = (ScriptObj)wep.Value.ScriptObject;
				if (obj != null) {
					//Console.WriteLine("Running events for script: " + wep.Value.Name);
					obj.RunEvents ();
				}
			}

			// Level-Npcs
			lock (TimerLock) {
				foreach (KeyValuePair<String, GraalLevel> lvl in LevelList) {
					//Console.WriteLine("Level: " + lvl.Value.Name);
					foreach (KeyValuePair<int, GraalLevelNPC> npc in lvl.Value.NpcList) {
						//Console.WriteLine("LevelScript: " + npc.Value.Script);
						ScriptObj obj = (ScriptObj)npc.Value.ScriptObject;
						if (obj != null)
							obj.RunEvents ();
					}
				}
			}
			
			Compiler.ManageCompilers ();
			while (Compiler.RunList.Count > 0) {
				IRefObject obj;
				lock (Compiler.RunList)
					obj = Compiler.RunList.Dequeue ();
				obj.Call ("onCreated", null);
				
			}

			// Make sure the port is open for NC-connections every 30 seconds
			if (this.milliseconds == 30000)
			{
				this.milliseconds = 0;
				this.UPnPOpenPort();
			}

			return true;
		}

		/// <summary>
		/// FindLevel
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		public GraalLevel FindLevel (String Name)
		{
			GraalLevel Level = null;
			if (!LevelList.TryGetValue (Name, out Level)) {
				Level = new GraalLevel (Name, this.TimerLock);
				lock (this.TimerLock) {
					LevelList [Name] = Level;
				}
			}

			return Level;
		}

		/// <summary>
		/// FindLevel
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		public GraalLevelNPC FindNPC (int id)
		{
			GraalLevelNPC NPC = null;
			foreach (KeyValuePair<string, GraalLevel> l in LevelList) {
				if (l.Value.NpcList.TryGetValue ((int)id, out NPC))
					continue;
			}


			return NPC;
		}	
	
		/// <summary>
		/// FindClass
		/// </summary>
		public ServerClass FindClass (String Name)
		{
			ServerClass Class = null;
			if (ClassList.TryGetValue (Name, out Class))
				return Class;
			return null;
		}
	
		/// <summary>
		/// FindWeapon
		/// </summary>
		public ServerWeapon FindWeapon (String Name)
		{
			ServerWeapon Weapon = null;
			if (WeaponList.TryGetValue (Name, out Weapon))
				return Weapon;
			return null;
		}

		/// <summary>
		/// Delete Class
		/// </summary>
		public bool DeleteClass (String ClassName)
		{
			if (ClassList.Remove (ClassName)) {
				SendPacket (new CString () + (byte)NCConnection.PacketOut.NC_CLASSDELETE + ClassName, null);
				// TODO: Send to GServer
				return true;
			}
			return false;
		}

		/// <summary>
		/// Delete Weapon
		/// </summary>
		public bool DeleteWeapon (String WeaponName)
		{
			if (WeaponList.Remove (WeaponName)) {
				SendGSPacket (new CString () + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.WEAPONDEL + WeaponName);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Set Class Data (and compile+exec)
		/// </summary>
		public int SetClass (String ClassName, String ClassScript, bool SendToGS)
		{
			// Set Weapon Data
			int Status = 0;
			ServerClass Class = null;
			if (ClassList.TryGetValue (ClassName, out Class))
			{
				Console.WriteLine("Class " + ClassName + " found. Script: " + ClassScript);
				Class.UpdateClass (ClassName, ClassScript);
				Status = 1;
			}
			else
			{
				Console.WriteLine("Class " + ClassName + " not found. Script: " + ClassScript);
				Class = new ServerClass (ClassName, ClassScript);
				ClassList [ClassName] = Class;
				SendPacket (new CString () + (byte)NCConnection.PacketOut.NC_CLASSADD + ClassName, null);
			}

			// Valid Weapon?
			if (Class != null) {
				// Send to GServer
				//if (SendToGS)
				//	this.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ. + (byte)ClassName.Length + ClassName + ClassScript);

				Compiler.CompileAdd(Class);
			}

			// Added or Updated?
			return Status;
		}

		/// <summary>
		/// Set Weapon Data (and compile+exec)
		/// </summary>
		public int SetWeapon (CSocket Server, String WeaponName, String WeaponImage, String WeaponCode, bool SendToGS)
		{
			// Set Weapon Data
			int Status = 0;
			ServerWeapon Weapon = null;
			if (WeaponList.TryGetValue (WeaponName, out Weapon)) {
				Weapon.UpdateWeapon (WeaponName, WeaponImage, WeaponCode);
				Status = 1;
			} else {
				Weapon = new ServerWeapon (Server, WeaponName, WeaponImage, WeaponCode);
				WeaponList [WeaponName] = Weapon;
			}

			// Valid Weapon?
			if (Weapon != null) {
				// Send to GServer
				if (SendToGS) {
					this.SendGSPacket (new CString () + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.WEAPONADD + (byte)WeaponName.Length + WeaponName + (byte)WeaponImage.Length + WeaponImage + WeaponCode);
					Compiler.CompileAdd (Weapon);
					/*
					CString output = new CString();
					output += new CString() + (byte)33 + (byte)WeaponName.Length + WeaponName
						+ (byte)0 + (byte)WeaponImage.Length + WeaponImage
						+ (byte)74 + (short)0 + "\n";

					CString header = new CString()+"weapon";

					// Get the mod time and send packet 197.
					CString smod = new CString() + (long)123123;
					smod.Tokenize();
					output += new CString() + (byte)197 + header + "," + smod + "\n";

					//System.IO.StreamReader sr = new System.IO.StreamReader(WeaponName.Replace("-","weapon").Replace("*","weapon")+"_ClientSide.dll");
					CString b = new CString() + "lol";
					//sr.Close();
					//sr.Dispose();
					
					// Add to the output stream.
					output += new CString() + (byte)100 + (int)b.Length + "\n";
					output += new CString() + b;

					foreach (GraalPlayer pl in this.PlayerManager)
					{
						if (pl.FindWeapon(WeaponName) != null)
						{
							this.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.FORWARDTOPLAYER + (short)pl.Id + output);
							Console.WriteLine(output);
						}
					}
					*/
				}
			}

			// Added or Updated?
			return Status;
		}

		/// <summary>
		/// Send Text to NPC-Controls
		/// </summary>
		public void SendNCChat (String Message)
		{
			this.SendNCChat (Message, null);
		}
		public void SendNCChat (String Message, NCConnection From)
		{
			SendPacket (new CString () + (byte)NCConnection.PacketOut.NC_CHAT + Message, From);
		}

		/// <summary>
		/// Send Private Message to Player
		/// </summary>
		public void SendPM (short PlayerId, String Message, bool IsTokenized)
		{
			SendGSPacket (new CString () + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.SENDPM + (short)PlayerId + "\"\"," + (IsTokenized ? Message : CString.tokenize (Message)));
		}

		/// <summary>
		/// Send Text to Remote-Controls
		/// </summary>
		public void SendRCChat (String Message)
		{
			SendGSPacket (new CString () + (byte)GServerConnection.PacketOut.RCCHAT + Message);
		}

		/// <summary>
		/// Send Packet to GServer
		/// </summary>
		public void SendGSPacket (CString Packet)
		{
			GSConn.SendPacket (Packet);
		}

		/// <summary>
		/// Send Packet to everyone but sender
		/// </summary>
		public void SendPacket (CString Packet, NCConnection Sender)
		{
			/*
			List<NCConnection> _tmpNCList = new List<NCConnection>();
			foreach (NCConnection nc in NCList)
			{
				_tmpNCList.Add(nc);
			}
			*/
			lock (NCList)
			{
				foreach (NCConnection nc in NCList)
				{
					if (nc != Sender)
					{
						if (nc.Connected)
						{
							Console.WriteLine("[NCOUT]: " + Packet.ToString());
							nc.SendPacket(Packet, true);
						}
						//else Console.WriteLine("Failed to send data to NC!");
					}
				}
			}
			//_tmpNCList.Clear ();
			//_tmpNCList = null;
		}
		#endregion
	}
}

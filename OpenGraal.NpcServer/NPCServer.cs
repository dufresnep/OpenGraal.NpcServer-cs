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
	public class Framework
	{
		#region Member variables
		/// <summary>
		/// Member Variables
		/// </summary>
		protected AsyncCallback cNCAccept;
		internal GameCompiler Compiler;
		public GServerConnection GSConn;
		protected TcpListener NCListen;
		
		protected int TimerId;
		//protected TimerEventHandler TimerHandle;
		internal object TimerLock = new object();
 
		// NC Variables
		public int NWTime = 0;
		public List<NCConnection> NCList = new List<NCConnection>();
		public String NCMsg;

		// Dictionarys
		public Dictionary<int, ScriptObj> ScriptList = new Dictionary<int, ScriptObj>();
		public Dictionary<String, GraalLevel> LevelList = new Dictionary<String, GraalLevel>();
		public Dictionary<String, ServerClass> ClassList = new Dictionary<String, ServerClass>();
		public Dictionary<String, ServerWeapon> WeaponList = new Dictionary<String, ServerWeapon>();
		public GraalPlayerList PlayerManager = null;
		public bool running = true;
		#endregion

		#region Constructor / Destructor
		/// <summary>
		/// Creates a new NPCServer
		/// </summary>
		internal Framework(string OptionsFile = "")
		{
			// Default PM
			this.NCMsg = CString.tokenize("I am the npcserver for\nthis game server. Almost\nall npc actions are controled\nby me.");

			// Create Compiler
			Compiler = new GameCompiler(this);

			// Create Player Manager
			PlayerManager = new GraalPlayerList();
			
			// Connect to GServer
			GSConn = new GServerConnection(this);
			GSConn.Connect("hosting.opengraal.com", 14900);
			if (GSConn.Connected)
			{
				GSConn.SendLogin("(npcserver)", "npcserver1", "NPC-Server (Server)");
				GSConn.ReceiveData();
			}

			// Setup NPC-Control Listener
			cNCAccept = new AsyncCallback(NCControl_Accept);
			NCListen = new TcpListener(IPAddress.Parse("0.0.0.0"), 14852);
			NCListen.Start();
			NCListen.BeginAcceptSocket(cNCAccept, NCListen);

			// Setup Timer
			//timeBeginPeriod(50);
			//TimerHandle = new TimerEventHandler(RunServer);
			//TimerId = timeSetEvent(50, 0, TimerHandle, IntPtr.Zero, EVENT_TYPE);
		}

		/// <summary>
		/// Detructor -> Cleanup
		/// </summary>
		~Framework()
		{
			this.running = false;
			//timeKillEvent(this.TimerId);
			//timeEndPeriod(50);
			
		}
		#endregion

		/// <summary>
		/// Accept incoming connection for NPC-Control
		/// </summary>
		/// <param name="ar"></param>
		protected void NCControl_Accept(IAsyncResult ar)
		{
			// Accept Socket
			NCConnection Client = new NCConnection(this, (Socket)NCListen.EndAcceptSocket(ar));
			Client.ReceiveData();
			NCList.Add(Client);

			// Listen for incoming connections
			NCListen.BeginAcceptSocket(cNCAccept, NCListen);
		}

		#region Public functions
		/// <summary>
		/// Run NPC Server
		/// </summary>
		public bool RunServer()
		{
			// Run Timeouts
			foreach (KeyValuePair<String, ServerWeapon> wep in WeaponList)
			{
				//
				ScriptObj obj = (ScriptObj)wep.Value.ScriptObject;
				if (obj != null)
				{
					//Console.WriteLine("Running events for script: " + wep.Value.Name);
					obj.RunEvents();
				}
			}

			// Level-Npcs
			lock (TimerLock)
			{
				foreach (KeyValuePair<String, GraalLevel> lvl in LevelList)
				{
					//Console.WriteLine("Level: " + lvl.Value.Name);
					foreach (KeyValuePair<int, GraalLevelNPC> npc in lvl.Value.NpcList)
					{
						//Console.WriteLine("LevelScript: " + npc.Value.Script);
						ScriptObj obj = (ScriptObj)npc.Value.ScriptObject;
						if (obj != null)
							obj.RunEvents();
					}
				}
			}
			Compiler.ManageCompilers();
			while (Compiler.RunList.Count > 0)
			{
				IRefObject obj;
				lock (Compiler.RunList)
					obj = Compiler.RunList.Dequeue();
				obj.Call("onCreated", null);
				
			}

			return true;
		}

		/// <summary>
		/// FindLevel
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		public GraalLevel FindLevel(String Name)
		{
			GraalLevel Level = null;
			if (!LevelList.TryGetValue(Name, out Level))
			{
				Level = new GraalLevel(Name, this.TimerLock);
				lock (this.TimerLock)
				{
					LevelList[Name] = Level;
				}
			}

			return Level;
		}
		
		/// <summary>
		/// FindClass
		/// </summary>
		public ServerClass FindClass(String Name)
		{
			ServerClass Class = null;
			if (ClassList.TryGetValue(Name, out Class))
				return Class;
			return null;
		}
	
		/// <summary>
		/// FindWeapon
		/// </summary>
		public ServerWeapon FindWeapon(String Name)
		{
			ServerWeapon Weapon = null;
			if (WeaponList.TryGetValue(Name, out Weapon))
				return Weapon;
			return null;
		}

		/// <summary>
		/// Delete Class
		/// </summary>
		public bool DeleteClass(String ClassName)
		{
			if (ClassList.Remove(ClassName))
			{
				SendPacket(new CString() + (byte)NCConnection.PacketOut.NC_CLASSDELETE + ClassName, null);
				// TODO: Send to GServer
				return true;
			}
			return false;
		}

		/// <summary>
		/// Delete Weapon
		/// </summary>
		public bool DeleteWeapon(String WeaponName)
		{
			if (WeaponList.Remove(WeaponName))
			{
				SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.WEAPONDEL + WeaponName);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Set Class Data (and compile+exec)
		/// </summary>
		public int SetClass(String ClassName, String ClassScript, bool SendToGS)
		{
			// Set Weapon Data
			int Status = 0;
			ServerClass Class = null;
			if (ClassList.TryGetValue(ClassName, out Class))
			{
				Class.UpdateClass(ClassName, ClassScript);
				Status = 1;
			}
			else
			{
				Class = new ServerClass(ClassName, ClassScript);
				ClassList[ClassName] = Class;
				SendPacket(new CString() + (byte)NCConnection.PacketOut.NC_CLASSADD + ClassName, null);
			}

			// Valid Weapon?
			if (Class != null)
			{
				// Send to GServer
				//if (SendToGS)
				//	this.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.WEAPONADD + (byte)ClassName.Length + ClassName + ClassScript);
			}

			// Added or Updated?
			return Status;
		}

		/// <summary>
		/// Set Weapon Data (and compile+exec)
		/// </summary>
		public int SetWeapon(CSocket Server, String WeaponName, String WeaponImage, String WeaponCode, bool SendToGS)
		{
			// Set Weapon Data
			int Status = 0;
			ServerWeapon Weapon = null;
			if (WeaponList.TryGetValue(WeaponName, out Weapon))
			{
				Weapon.UpdateWeapon(WeaponName, WeaponImage, WeaponCode);
				Status = 1;
			}
				else
			{
				Weapon = new ServerWeapon(Server, WeaponName, WeaponImage, WeaponCode);
				WeaponList[WeaponName] = Weapon;
			}

			// Valid Weapon?
			if (Weapon != null)
			{
				// Send to GServer
				if (SendToGS)
				{
					this.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.WEAPONADD + (byte)WeaponName.Length + WeaponName + (byte)WeaponImage.Length + WeaponImage + WeaponCode);
					Compiler.CompileAdd(Weapon);
				}
			}

			// Added or Updated?
			return Status;
		}

		/// <summary>
		/// Send Text to NPC-Controls
		/// </summary>
		public void SendNCChat(String Message) { this.SendNCChat(Message, null); }
		public void SendNCChat(String Message, NCConnection From)
		{
			SendPacket(new CString() + (byte)NCConnection.PacketOut.NC_CHAT + Message, From);
		}

		/// <summary>
		/// Send Private Message to Player
		/// </summary>
		public void SendPM(short PlayerId, String Message, bool IsTokenized)
		{
			GSConn.SendPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.SENDPM + (short)PlayerId + "\"\"," + (IsTokenized ? Message : CString.tokenize(Message)));
		}

		/// <summary>
		/// Send Text to Remote-Controls
		/// </summary>
		public void SendRCChat(String Message)
		{
			GSConn.SendPacket(new CString() + (byte)GServerConnection.PacketOut.RCCHAT + Message);
		}

		/// <summary>
		/// Send Packet to GServer
		/// </summary>
		public void SendGSPacket(CString Packet)
		{
			GSConn.SendPacket(Packet);
		}

		/// <summary>
		/// Send Packet to everyone but sender
		/// </summary>
		public void SendPacket(CString Packet, NCConnection Sender)
		{
			foreach (NCConnection nc in NCList)
			{
				if (nc != Sender)
					nc.SendPacket(Packet, true);
			}
		}
		#endregion
	}
}

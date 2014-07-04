using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.NpcServer;
using OpenGraal.Common.Players;
using OpenGraal.Common.Levels;

namespace OpenGraal.NpcServer
{
	public class GServerConnection : CSocket, OpenGraal.NpcServer.IGServerConnection
	{
		/// <summary>
		/// Enumerator -> Packet In
		/// </summary>
		public enum PacketIn
		{
			LEVELLINKS = 1,
			LEVELNPCPROPS = 3,
			LEVELSIGNS = 5,
			LEVELNAME = 6,
			LEVELMODTIME = 39,
			LEVELNPCDELETE = 150,
			LEVELSETACTIVE = 156,
			
			OTHERPLPROPS = 8,
			PLAYERPROPS = 9,
			PLFLAGSET = 28,
			NPCWEAPONADD = 33,
			PLWEAPONMNG = 34,
			PRIVATEMESSAGE = 37,
			NEWWORLDTIME = 42,
			TRIGGERACTION = 48,
			PLWEAPONSSET = 60,
			NC_CONTROL = 78,
		};

		/// <summary>
		/// Enumerator -> Packet Out
		/// </summary>
		public enum PacketOut
		{
			NPCLOG = 0,
			GETWEAPONS = 1,
			GETLEVELS = 2,
			SENDPM = 3,
			SENDTORC = 4,
			WEAPONADD = 5,
			WEAPONDEL = 6,
			PLAYERPROPSSET = 7,
			PLAYERWEAPONSGET = 8,
			PLAYERPACKET = 9,
			PLAYERWEAPONADD = 10,
			PLAYERWEAPONDEL = 11,
			LEVELGET = 12,
			NPCPROPSSET = 13,
			NPCWARP = 14,
			SENDRPGMESSAGE = 15,
			PLAYERFLAGSET = 16,
			SAY2SIGN = 17,
			PLAYERSTATUSSET = 18,
			NPCMOVE = 19,
			PLAYERPROPS = 2,
			RCCHAT = 79,
			NCQUERY = 103,
		};

		/// <summary>
		/// Enumerator -> NCQuery Packets
		/// </summary>
		public enum NCREQ
		{
			NPCLOG			= 0,
			GETWEAPONS		= 1,
			GETLEVELS		= 2,
			SENDPM			= 3,
			SENDTORC		= 4,
			WEAPONADD		= 5,
			WEAPONDEL		= 6,
			PLSETPROPS		= 7,
			PLGETWEPS		= 8,
			PLSNDPACKET		= 9,
			PLADDWEP		= 10,
			PLDELWEP		= 11,
			LEVELGET		= 12,
			NPCPROPSET		= 13,
			NPCWARP			= 14,
			PLRPGMSG		= 15,
			PLSETFLAG		= 16,
			PLMSGSIGN		= 17,
			PLSETSTATUS		= 18,
			NPCMOVE			= 19,
			FORWARDTOPLAYER = 20,
		};
		public enum NCI
		{
			PLAYERWEAPONS		= 0,
			PLAYERWEAPONADD		= 1,
			PLAYERWEAPONDEL		= 2,
			GMAPLIST			= 3,
		};
		/// <summary>
		/// Member Variables
		/// </summary>
		protected Framework Server;
		protected Players.Player NCPlayer;
		protected GraalLevel ActiveLevel = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public GServerConnection (Framework Server)
		{
			this.Server = Server;
		}

		/// <summary>
		/// Send Login Information
		/// </summary>
		public void SendLogin (String Account, String Password, String Nickname)
		{
			// Send Login
			CString LoginPacket = new CString () + (byte)2 + "GRNS0000" + (byte)Account.Length + Account + (byte)Password.Length + Password + (short)AppSettings.GetInstance ().NCPort + "\n"
				+ (byte)PacketOut.PLAYERPROPS + (byte)0 + (byte)Nickname.Length + Nickname + "\n";
			LoginPacket.ZCompress ().PreLength ();
			this.Send (LoginPacket.Buffer);
		}

		/// <summary>
		/// Handle Received Data
		/// </summary>
		protected override void HandleData (CString Packet)
		{
			while (Packet.BytesLeft > 0) {
				// Grab Single Packet
				CString CurPacket = Packet.ReadString ('\n');

				// Read Packet Type
				int PacketId = CurPacket.ReadGUByte1 ();

				// Call Packet Callback
				//RemoteControl.CallCallBack(PacketId, (CString)CurPacket.DeepClone());

				// Run Internal Packet Function
				switch ((PacketIn)PacketId) {
				// Packet 6 - Set Active Level & Clear
				case PacketIn.LEVELNAME:
					ActiveLevel = Server.FindLevel (CurPacket.ReadString ().Text);
					Console.WriteLine (ActiveLevel.Name);
						//ActiveLevel.Clear();
					break;

				// Paceket 1 - Level Links
				case PacketIn.LEVELLINKS:
					break;

				// Packet 3 - Level NPC Props
				case PacketIn.LEVELNPCPROPS:
					{
						int npcId = CurPacket.ReadGByte3 ();
						//Console.WriteLine("npc-id("+npcId.ToString()+")");
						if (ActiveLevel != null) {
	
							GraalLevelNPC test = ActiveLevel.GetNPC (this.Server.GSConn, npcId);
							test.npcserver = true;
							test.SetProps (CurPacket);
							/*
							foreach (NCConnection nc in Server.NCList)
							{
								nc.SendPacket(new CString() + (byte)NCConnection.PacketOut.NC_NPCADD + (int)test.Id + (byte)50 + (byte)test.Nickname.Length + test.Nickname + (byte)51 + (byte)("OBJECT".Length) + "OBJECT" + (byte)52 + (byte)test.Level.Name.Length + test.Level.Name);
							}
							*/
							this.Server.Compiler.CompileAdd (test);
						}
						break;
					}

				// Packet 5 - Level Signs
				case PacketIn.LEVELSIGNS:
					break;

				// Packet 39 - Level Mod Time
				case PacketIn.LEVELMODTIME:
					if (ActiveLevel != null)
						ActiveLevel.SetModTime (CurPacket.ReadGUByte5 ());
					break;

				// Packet 150 - Delete Level NPC
				case PacketIn.LEVELNPCDELETE:
					Server.FindLevel (CurPacket.ReadChars (CurPacket.ReadGUByte1 ())).DeleteNPC (CurPacket.ReadGByte3 ());
					break;

				// Packet 156 - Set Active Level
				case PacketIn.LEVELSETACTIVE:
					ActiveLevel = Server.FindLevel (CurPacket.ReadString ().Text);
					break;

				// Add Player & Set Props
				case PacketIn.OTHERPLPROPS:
					{
						Players.Player Player = (Players.Player)Server.PlayerManager.AddPlayer (CurPacket.ReadGByte2 (), this);
						if (Player != null) {
							Player.SetProps (CurPacket);
							
						}
						break;
					}

				case PacketIn.PLFLAGSET:
					{
						Players.Player Player = (Players.Player)Server.PlayerManager.AddPlayer (CurPacket.ReadGByte2 ());
						String FlagName = CurPacket.ReadString ('=').Text;
						String FlagValue = CurPacket.ReadString ().Text;
						if (Player != null)
							Player.Flags [FlagName] = FlagValue;
						break;
					}

				case PacketIn.PLAYERPROPS:
					if (NCPlayer == null)
						NCPlayer = (Players.Player)Server.PlayerManager.AddPlayer (0);
					NCPlayer.SetProps (CurPacket);
					break;

				// Add weapon to list
				case PacketIn.NPCWEAPONADD:
					{
						String WeaponName = CurPacket.ReadChars (CurPacket.ReadGUByte1 ());
						String WeaponImage = CurPacket.ReadChars (CurPacket.ReadGUByte1 ());
						String WeaponScript = CurPacket.ReadString ().Text;
						Server.SetWeapon (this, WeaponName, WeaponImage, WeaponScript, false);
						/*
					foreach (Players.Player p in this.Server.PlayerManager) {
							this.Server.SendGSPacket (new CString () + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLGETWEPS + (short)p.Id);
						}
					*/
						break;
					}

				// Add/Remove weapon from Player
				case PacketIn.PLWEAPONMNG:
					{
						Players.Player Player = (Players.Player)Server.PlayerManager.AddPlayer (CurPacket.ReadGByte2 ());
						if (Player != null) {
							bool addWeapon = (CurPacket.ReadGByte1 () > 0);
							String name = CurPacket.ReadString ().Text;
							if (addWeapon)
								Player.AddWeapon (name);
							else
								Player.DeleteWeapon (name, false);
						}

						break;
					}

				case PacketIn.PLWEAPONSSET:
					{
						Players.Player Player = (Players.Player)Server.PlayerManager.AddPlayer (CurPacket.ReadGByte2 ());
						if (Player != null) {
							while (CurPacket.BytesLeft > 0)
								Player.AddWeapon (CurPacket.ReadChars (CurPacket.ReadGUByte1 ()));
						}
						break;
					}

				case PacketIn.NEWWORLDTIME: // Remove Class from List
					Server.NWTime = CurPacket.ReadGByte4 ();
						//RemoteControl.ClassList.Remove(CurPacket.ReadString().Text);
					break;

				// Private Message
				case PacketIn.PRIVATEMESSAGE:
					short PlayerId = CurPacket.ReadGByte2 ();
					CString Message = CurPacket.ReadString ();
					Server.SendPM (PlayerId, Server.NCMsg, true);
					break;
				case PacketIn.TRIGGERACTION:
					short _pid = CurPacket.ReadGByte2 ();
					int _npcId = (int)CurPacket.readGUInt ();
					float x = (float)CurPacket.readGUChar () / 2.0f;
					float y = (float)CurPacket.readGUChar () / 2.0f;
					string action = CurPacket.ReadString (',').ToString ().Trim ();
					string[] _params = CurPacket.ReadString ().ToString ().Split (',');
					Console.Write ("Call npcid(" + _npcId.ToString () + ") onAction" + action + "(_params); _params: ");
					foreach (string p in _params)
						Console.Write (p + ", ");
					Console.WriteLine ();
					if (_npcId != 0) {
						GraalLevelNPC tmpNpc = this.Server.PlayerManager.FindPlayer (_pid).Level.GetNPC (_npcId);

						if (tmpNpc == null)
							Console.WriteLine ("npc cannot be found! :'(");

						if (tmpNpc != null)
							tmpNpc.Call ("onAction" + action, new object[] { this.Server.PlayerManager.FindPlayer (_pid), _params });
					} else {
						this.Server.PlayerManager.FindPlayer (_pid).CallNPCs ("onAction" + action, new object[] {
							this.Server.PlayerManager.FindPlayer (_pid),
							_params
						}
						);
						this.Server.PlayerManager.FindPlayer (_pid).Level.CallNPCs ("onAction" + action, new object[] {
							this.Server.PlayerManager.FindPlayer (_pid),
							_params
						}
						);
					}
					break;
				case PacketIn.NC_CONTROL:
					System.Console.Write ("GSCONN -> Packet [" + (PacketIn)PacketId + "]: ");
					Server.SendGSPacket (new CString () + (byte)PacketOut.GETWEAPONS);
					Server.SendGSPacket (new CString () + (byte)PacketOut.GETLEVELS);
					int PacketId2 = CurPacket.ReadGUByte1 ();
					switch ((NCI)PacketId2) {
					case NCI.PLAYERWEAPONS: // 0
						{
							Players.Player Player = (Players.Player)Server.PlayerManager.AddPlayer (CurPacket.ReadGByte2 ());
							if (Player != null) {
								Console.WriteLine (Player.Account);
								CString weapons = CurPacket.Untokenize ();
								string[] weaponsarr = weapons.ToString ().Split ('\n');
								foreach (string tmpWeap in weaponsarr) {
									//string tmpWeap = CurPacket.ReadChars(CurPacket.ReadGUByte1());
									Console.WriteLine ("Weapon: " + tmpWeap);
									if (Player.FindWeapon (tmpWeap) == null) {
										Console.WriteLine ("Not Found on player");
										Common.Scripting.ServerWeapon tmpWeap2 = this.Server.FindWeapon (tmpWeap);
										if (tmpWeap2 != null) {
											Console.WriteLine ("Adding");
											Player.AddWeapon (tmpWeap2);
										} else
											Console.WriteLine ("Weapon not found in system. :'(");
									}
								}
							}
							break;
						}

					case NCI.PLAYERWEAPONADD:
						System.Console.WriteLine (" ADDWEAPON");
						Players.Player _Player = (Players.Player)Server.PlayerManager.AddPlayer (CurPacket.ReadGByte2 ());
						if (_Player != null) {
							System.Console.Write ("Player: " + _Player.Account);
							bool addWeapon = (CurPacket.ReadGByte1 () > 0);
							String name = CurPacket.ReadString ().Text;
							System.Console.WriteLine (" Name: " + name);
							Common.Scripting.ServerWeapon tmpWp = this.Server.FindWeapon (name);
							if (addWeapon)
								_Player.AddWeapon (tmpWp);
							else
								_Player.DeleteWeapon (name, false);
						}
						break;
					default:
						System.Console.WriteLine ("[" + PacketId2 + "]: " + CurPacket.ReadString ().Text);
						break;
					}
					break;
				default:
					System.Console.WriteLine ("GSCONN -> Packet [" + (PacketIn)PacketId + "]: " + CurPacket.ReadString ().Text);
					break;
				}
			}
		}
	}
}

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
	public class GServerConnection : CSocket
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
			PLWEAPONSSET = 60,
		};

		/// <summary>
		/// Enumerator -> Packet Out
		/// </summary>
		public enum PacketOut
		{
			PLAYERPROPS = 2,
			RCCHAT = 79,
			NCQUERY = 103,
		};

		/// <summary>
		/// Enumerator -> NCQuery Packets
		/// </summary>
		public enum NCREQ
		{
			NPCLOG		= 0,
			GETWEAPONS	= 1,
			GETLEVELS	= 2,
			SENDPM		= 3,
			SENDTORC	= 4,
			WEAPONADD   = 5,
			WEAPONDEL	= 6,
			PLSETPROPS	= 7,
			PLGETWEPS	= 8,
			PLSNDPACKET	= 9,
			PLADDWEP	= 10,
			PLDELWEP	= 11,
			LEVELGET	= 12,
			NPCPROPSET	= 13,
			NPCWARP		= 14,
			PLRPGMSG	= 15,
			PLSETFLAG	= 16,
			PLMSGSIGN	= 17,
			PLSETSTATUS = 18,
			NPCMOVE		= 19,
		};

		/// <summary>
		/// Member Variables
		/// </summary>
		protected Framework Server;
		protected GraalPlayer NCPlayer;
		protected GraalLevel ActiveLevel = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public GServerConnection(Framework Server)
		{
			this.Server = Server;
		}

		/// <summary>
		/// Send Login Information
		/// </summary>
		public void SendLogin(String Account, String Password, String Nickname)
		{
			// Send Login
			CString LoginPacket = new CString() + (byte)2 + "GRNS0000" + (byte)Account.Length + Account + (byte)Password.Length + Password + (short)14852 + "\n"
				+  (byte)PacketOut.PLAYERPROPS + (byte)0 + (byte)Nickname.Length + Nickname + "\n";
			LoginPacket.ZCompress().PreLength();
			this.Send(LoginPacket.Buffer);
		}

		/// <summary>
		/// Handle Received Data
		/// </summary>
		protected override void HandleData(CString Packet)
		{
			while (Packet.BytesLeft > 0)
			{
				// Grab Single Packet
				CString CurPacket = Packet.ReadString('\n');

				// Read Packet Type
				int PacketId = CurPacket.ReadGUByte1();

				// Call Packet Callback
				//RemoteControl.CallCallBack(PacketId, (CString)CurPacket.DeepClone());

				// Run Internal Packet Function
				switch ((PacketIn)PacketId)
				{
					// Packet 6 - Set Active Level & Clear
					case PacketIn.LEVELNAME:
						ActiveLevel = Server.FindLevel(CurPacket.ReadString().Text);
						//ActiveLevel.Clear();
						break;

					// Paceket 1 - Level Links
					case PacketIn.LEVELLINKS:
						break;

					// Packet 3 - Level NPC Props
					case PacketIn.LEVELNPCPROPS:
					{
						int npcId = CurPacket.ReadGByte3();
						if (ActiveLevel != null)
							ActiveLevel.GetNPC(npcId).SetProps(CurPacket);
						break;
					}

					// Packet 5 - Level Signs
					case PacketIn.LEVELSIGNS:
						break;

					// Packet 39 - Level Mod Time
					case PacketIn.LEVELMODTIME:
						if (ActiveLevel != null)
							ActiveLevel.SetModTime(CurPacket.ReadGUByte5());
						break;

					// Packet 150 - Delete Level NPC
					case PacketIn.LEVELNPCDELETE:
						Server.FindLevel(CurPacket.ReadChars(CurPacket.ReadGUByte1())).DeleteNPC(CurPacket.ReadGByte3());
						break;

					// Packet 156 - Set Active Level
					case PacketIn.LEVELSETACTIVE:
						ActiveLevel = Server.FindLevel(CurPacket.ReadString().Text);
						break;

					// Add Player & Set Props
					case PacketIn.OTHERPLPROPS:
					{
						GraalPlayer Player = Server.PlayerManager.AddPlayer(CurPacket.ReadGByte2());
						if (Player != null)
							Player.SetProps(CurPacket);
						break;
					}

					case PacketIn.PLFLAGSET:
					{
						GraalPlayer Player = Server.PlayerManager.AddPlayer(CurPacket.ReadGByte2());
						String FlagName = CurPacket.ReadString('=').Text;
						String FlagValue = CurPacket.ReadString().Text;
						if (Player != null)
							Player.flags[FlagName] = FlagValue;
						break;
					}

					//case PacketIn.PLAYERPROPS:
					//	if (NCPlayer == null)
					//		NCPlayer = Server.PlayerManager.AddPlayer(0);
					//	NCPlayer.SetProps(CurPacket);
					//	break;

					// Add weapon to list
					case PacketIn.NPCWEAPONADD:
					{
						String WeaponName = CurPacket.ReadChars(CurPacket.ReadGUByte1());
						String WeaponImage = CurPacket.ReadChars(CurPacket.ReadGUByte1());
						String WeaponScript = CurPacket.ReadString().Text;
						Server.SetWeapon(WeaponName, WeaponImage, WeaponScript, false);
						break;
					}

					// Add/Remove weapon from Player
					case PacketIn.PLWEAPONMNG:
					{
						GraalPlayer Player = Server.PlayerManager.AddPlayer(CurPacket.ReadGByte2());
						if (Player != null)
						{
							bool addWeapon = (CurPacket.ReadGByte1() > 0);
							String name = CurPacket.ReadString().Text;
							if (addWeapon)
								Player.AddWeapon(name);
							else
								Player.DeleteWeapon(name, false);
						}

						break;
					}

					case PacketIn.PLWEAPONSSET:
					{
						GraalPlayer Player = Server.PlayerManager.AddPlayer(CurPacket.ReadGByte2());
						if (Player != null)
						{
							while (CurPacket.BytesLeft > 0)
								Player.AddWeapon(CurPacket.ReadChars(CurPacket.ReadGUByte1()));
						}
						break;
					}

					case PacketIn.NEWWORLDTIME: // Remove Class from List
						Server.NWTime = CurPacket.ReadGByte4();
						//RemoteControl.ClassList.Remove(CurPacket.ReadString().Text);
						break;

					// Private Message
					case PacketIn.PRIVATEMESSAGE:
						short PlayerId = CurPacket.ReadGByte2();
						CString Message = CurPacket.ReadString();
						Server.SendPM(PlayerId, Server.NCMsg, true);
						break;

					default:
						System.Console.WriteLine("GSCONN -> Packet [" + PacketId + "]: " + CurPacket.ReadString().Text);
						break;
				}
			}
		}
	}
}

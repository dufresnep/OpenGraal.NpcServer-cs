using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.NpcServer;

/*
public void onCreated()
{
  var npc = FindWeapon("-gr_movement");
  SendToRC("TEST: " + npc.name + " - " + IMCOOL());
}

public string IMCOOL()
{
  return "RUCOOL?";
}

public void onCreated()
{
  foreach (GraalPlayer pl in players)
    SendToRC("Player " + pl.Id + ": " + pl.Account);
}

public string IMCOOL()
{
  return "RUCOOL?";
}

*/

namespace OpenGraal.NpcServer
{
	public class GraalPlayerOld
	{
		/// <summary>
		/// Player Properties Enum
		/// </summary>
		internal enum Properties
		{
			NICKNAME	= 0,
			MAXPOWER	= 1,
			CURPOWER	= 2,
			RUPEESCOUNT	= 3,
			ARROWSCOUNT	= 4,
			BOMBSCOUNT	= 5,
			GLOVEPOWER	= 6,
			BOMBPOWER	= 7,
			SWORDPOWER	= 8,
			SHIELDPOWER	= 9,
			ANIMATION	= 10,
			HEADIMAGE	= 11,
			CURCHAT		= 12,
			PLCOLORS	= 13,
			PLAYERID	= 14,
			PLAYERX		= 15,
			PLAYERY		= 16,
			PLSPRITE	= 17,
			PLSTATUS	= 18,
			CARRYSPRITE	= 19,
			CURLEVEL	= 20,
			HORSEIMG	= 21,
			HORSEBUSHES	= 22,
			EFFECTCOLOR	= 23,
			CARRYNPC	= 24,
			APCOUNTER	= 25,
			MAGICPOINTS	= 26,
			KILLSCOUNT	= 27,
			DEATHSCOUNT	= 28,
			ONLINESECS	= 29,
			IPADDRESS	= 30,
			UDPPORT		= 31,
			ALIGNMENT	= 32,
			ADDITFLAGS	= 33,
			ACCOUNTNAME	= 34,
			BODYIMAGE	= 35,
			PLRATING	= 36,
			GATTRIB1	= 37,
			GATTRIB2	= 38,
			GATTRIB3	= 39,
			GATTRIB4	= 40,
			GATTRIB5	= 41,
			PLATTACHNPC = 42,
			GMAPLEVELX	= 43,
			GMAPLEVELY	= 44,
			PLAYERZ		= 45,
			GATTRIB6	= 46,
			GATTRIB7	= 47,
			GATTRIB8	= 48,
			GATTRIB9	= 49,
			JOINLEAVELV	= 50,
			CONNECTED	= 51,
			LANGUAGE	= 52,
			STATUSMSG	= 53,
			GATTRIB10	= 54,
			GATTRIB11	= 55,
			GATTRIB12	= 56,
			GATTRIB13	= 57,
			GATTRIB14	= 58,
			GATTRIB15	= 59,
			GATTRIB16	= 60,
			GATTRIB17	= 61,
			GATTRIB18	= 62,
			GATTRIB19	= 63,
			GATTRIB20	= 64,
			GATTRIB21	= 65,
			GATTRIB22	= 66,
			GATTRIB23	= 67,
			GATTRIB24	= 68,
			GATTRIB25	= 69,
			GATTRIB26	= 70,
			GATTRIB27	= 71,
			GATTRIB28	= 72,
			GATTRIB29	= 73,
			GATTRIB30	= 74,
			OSTYPE		= 75,
			TEXTCODEPG	= 76,
			UNKNOWN77	= 77,
			PIXELX		= 78,
			PIXELY		= 79,
			PIXELZ		= 80,
			UNKNOWN81	= 81,
			COMMUNITY	= 82,
		};

		/// <summary>
		/// Player Status Enum
		/// </summary>
		internal enum Status
		{
			PAUSED = 0x01,
			HIDDEN = 0x02,
			MALE = 0x04,
			DEAD = 0x08,
			ALLOWWEAPONS = 0x10,
			HASSPIN = 0x40
		}

		/// <summary>
		/// Member Variables
		/// </summary>
		internal Framework Server;
		
		/// <summary>
		/// Set Variables
		/// </summary>
		protected ColorManager ColorManager = null;
		protected FlagManager FlagManager = null;
		protected GraalLevel PlayerLevel = null;
		protected List<ServerWeapon> WeaponList = new List<ServerWeapon>();
		protected double Hearts = 3;
		protected int Ap = 50, Arrows = 10, Bombs = 5, BombPower = 1, Deaths = 0, Dir = 2, FullHearts = 3, GlovePower = 1, Gralats = 0, Kills = 0, Magic = 0, OnlineTime = 0, PlayerStatus = 0, ShieldPower = 1, SwordPower = 1, GmapX = 0, GmapY = 0, PixelX = 0, PixelY = 0;
		protected short Id = 0;
		protected string Account, Ani, BodyImage, Chat, HeadImage, Guild, IPAddress, Level = String.Empty, Nickname, ShieldImage, SwordImage;
		protected static int[] touchtestd = new int[] {24,8, 0,32, 24,56, 48,32};

		/// <summary>
		/// Constructor by Account / Id
		/// </summary>
		internal GraalPlayer(Framework Server, Int16 Id)
		{
			this.Server = Server;
			this.Id = Id;
			this.ColorManager = new ColorManager(new ColorManager.dSendColors(SendColors));
			this.FlagManager = new FlagManager(new FlagManager.dSendFlag(SendFlag));
		}

		/// <summary>
		/// Send Colors
		/// </summary>
		internal void SendColors()
		{
			this.SendProp(Properties.PLCOLORS);
		}

		/// <summary>
		/// Send Flag to NPCServer
		/// </summary>
		internal void SendFlag(String FlagName, String FlagValue)
		{
			// fix packet for flag
			Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLSETFLAG + (short)Id + FlagName + "=" + FlagValue);
		}

		/// <summary>
		/// Send Prop to GServer
		/// </summary>
		internal void SendProp(Properties PropId)
		{
			Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLSETPROPS + (short)this.Id + (byte)PropId + GetProp(PropId));
		}

		/// <summary>
		/// Set Player Guild
		/// </summary>
		internal void SetGuild(string GuildName)
		{
			int guildStart, guildEnd;
			if ((guildStart = Nickname.IndexOf('(')) >= 0)
			{
				if ((guildEnd = Nickname.IndexOf(')', guildStart)) >= 0)
					Nickname = Nickname.Substring(0, guildStart).TrimEnd();
			}

			if (GuildName != String.Empty)
				Nickname += " (" + GuildName + ")";
		}

		/// <summary>
		/// Set Player Nickname
		/// </summary>
		/// <param name="Nickname"></param>
		internal void SetNick(string Nickname)
		{
			// Set Nickname
			this.Nickname = Nickname;
			this.Guild = "";

			// Parse Guild
			int len = Nickname.LastIndexOf('(');
			if (len != -1)
			{
				int len2 = Nickname.LastIndexOf(')');
				if (len2 > len)
					this.Guild = Nickname.Substring(len + 1, len2 - len - 1);
			}
		}

		/// <summary>
		/// Get Property
		/// </summary>
		internal CString GetProp(Properties PropId)
		{
			switch (PropId)
			{
				case Properties.NICKNAME: // 0
					return new CString() + (byte)this.Nickname.Length + this.Nickname;

				case Properties.MAXPOWER: // 1
					return new CString() + (byte)this.FullHearts;

				case Properties.CURPOWER: // 2
					return new CString() + (byte)(this.Hearts * 2);

				case Properties.RUPEESCOUNT: // 3
					return new CString().WriteGByte3(this.Gralats);

				case Properties.ARROWSCOUNT: // 4
					return new CString() + (byte)this.Arrows;

				case Properties.BOMBSCOUNT: // 5
					return new CString() + (byte)this.Bombs;

				case Properties.GLOVEPOWER: // 6
					return new CString() + (byte)this.GlovePower;

				case Properties.BOMBPOWER: // 7
					return new CString() + (byte)this.BombPower;

				case Properties.SWORDPOWER: // 8
					return new CString() + (byte)(this.SwordPower + 30) + (byte)this.SwordImage.Length + this.SwordImage;

				case Properties.SHIELDPOWER: // 9
					return new CString() + (byte)(this.ShieldPower + 10) + (byte)this.ShieldImage.Length + this.ShieldImage;

				case Properties.ANIMATION: // 10
					return new CString() + (byte)this.Ani.Length + this.Ani;

				case Properties.HEADIMAGE: // 11
					return new CString() + (byte)(this.HeadImage.Length+100) + this.HeadImage;

				case Properties.CURCHAT: // 12
					return new CString() + (byte)this.Chat.Length + this.Chat;

				case Properties.PLCOLORS: // 13
					return ColorManager.GetPacket();

				case Properties.PLAYERX: // 15
					return new CString() + (byte)(this.PixelX / 8);

				case Properties.PLAYERY: // 16
					return new CString() + (byte)(this.PixelY / 8);

				case Properties.PLSPRITE: // 17
					return new CString() + (byte)(this.Dir % 4);

				case Properties.PLSTATUS: // 8
					return new CString() + (byte)this.PlayerStatus;

				case Properties.KILLSCOUNT: // 27
					return new CString() + (int)this.Kills;

				case Properties.DEATHSCOUNT: // 28
					return new CString() + (int)this.Deaths;

				case Properties.ONLINESECS: // 29
					return new CString() + (int)this.OnlineTime;

				case Properties.ALIGNMENT: // 32
					return new CString() + (byte)this.Ap;

				case Properties.BODYIMAGE: // 35
					return new CString() + (byte)this.BodyImage.Length + this.BodyImage;

				case Properties.PIXELX: // 75
				{
					int res = (PixelX < 0 ? -PixelX : PixelX) << 1;
					if (PixelX < 0)
						res |= 0x0001;
					return new CString() + (short)res;
				}

				case Properties.PIXELY: // 76
				{
					int res = (PixelY < 0 ? -PixelY : PixelY) << 1;
					if (PixelY < 0)
						res |= 0x0001;
					return new CString() + (short)res;
				}

				default:
					return new CString();
			}
		}

		/// <summary>
		/// Set Properties
		/// </summary>
		internal void SetProps(CString Packet)
		{
			bool moved = false;
			while (Packet.BytesLeft > 0)
			{
				Int32 PropId = Packet.ReadGUByte1();

				switch ((Properties)PropId)
				{
					case Properties.NICKNAME: // 0
						this.SetNick(Packet.ReadChars(Packet.ReadGUByte1()));
						break;

					case Properties.MAXPOWER: // 1
						this.FullHearts = Packet.ReadGUByte1();
						break;

					case Properties.CURPOWER: // 2
						this.Hearts = (double)Packet.ReadGUByte1() / 2;
						break;

					case Properties.RUPEESCOUNT: // 3
						this.Gralats = (int)Packet.ReadGUByte3();
						break;

					case Properties.ARROWSCOUNT: // 4
						this.Arrows = Packet.ReadGUByte1();
						break;

					case Properties.BOMBSCOUNT: // 5
						this.Bombs = Packet.ReadGUByte1();
						break;

					case Properties.GLOVEPOWER: // 6
						this.GlovePower = Packet.ReadGUByte1();
						break;

					case Properties.BOMBPOWER: // 7
						this.BombPower = Packet.ReadGUByte1();
						break;

					case Properties.SWORDPOWER: // 8
					{
						Int32 sp = Packet.ReadGUByte1();
						if (sp > 4)
						{
							sp -= 30;
							Int32 len = Packet.ReadGUByte1();
							if (len > 0)
								this.SwordImage = Packet.ReadChars(len);
						}
						else this.SwordImage = "sword" + sp + ".png";
						
						this.SwordPower = sp;
						break;
					}

					case Properties.SHIELDPOWER: // 9
					{
						Int32 sp = Packet.ReadGUByte1();
						if (sp > 3)
						{
							sp -= 10;
							if (sp < 0) break;
							Int32 len = Packet.ReadGUByte1();
							if (len > 0)
								this.ShieldImage = Packet.ReadChars(len);
						}
						else this.ShieldImage = "shield" + sp + ".png";

						this.ShieldPower = sp;
						break;
					}

					case Properties.ANIMATION: // 10
						this.Ani = Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.HEADIMAGE: // 11
					{
						Int32 len = Packet.ReadGUByte1();
						this.HeadImage = (len < 100 ? "head" + len + ".png" : Packet.ReadChars(len - 100));
						break;
					}

					case Properties.CURCHAT: // 12
						this.Chat = Packet.ReadChars(Packet.ReadGUByte1());
						if (PlayerLevel != null)
							PlayerLevel.CallNPCs("onPlayerChats", new object[] { this });
						break;

					case Properties.PLCOLORS: // 13
						for (int i = 0; i < 5; i++)
							this.ColorManager[i] = Packet.ReadGUByte1();
						break;

					case Properties.PLAYERID: // 14
						Packet.ReadGUByte2();
						break;

					case Properties.PLAYERX: // 15 - obsolete
						Packet.ReadGUByte1();
						//this.X = ((double)Packet.ReadGUByte1() / 2);
						//moved = true;
						break;

					case Properties.PLAYERY: // 16 - obsolete
						Packet.ReadGUByte1();
						//this.Y = ((double)Packet.ReadGUByte1() / 2);
						//moved = true;
						break;

					case Properties.PLSPRITE: // 17
						this.Dir = Packet.ReadGUByte1() % 4;
						break;

					case Properties.PLSTATUS: // 18
						this.PlayerStatus = Packet.ReadGUByte1();
						if ((this.PlayerStatus & (int)Status.DEAD) != 0)
						{
							if (PlayerLevel != null)
								PlayerLevel.CallNPCs("onPlayerDies", new object[] { this });
						}
						break;

					case Properties.CARRYSPRITE: // 19
						Packet.ReadGUByte1();
						break;

					case Properties.CURLEVEL: // 20
						this.Level = Packet.ReadChars(Packet.ReadGUByte1()).Trim();
						if (this.PlayerLevel != null)
							PlayerLevel.DeletePlayer(this);
						PlayerLevel = Server.FindLevel(this.Level);
						PlayerLevel.AddPlayer(this);
						break;

					case Properties.HORSEIMG: // 21
						Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.HORSEBUSHES: // 22
						Packet.ReadGUByte1();
						break;

					case Properties.EFFECTCOLOR: // 23
					{
						int len = Packet.ReadGUByte1();
						if (len > 0)
							Packet.ReadGUByte4();
						break;
					}

					case Properties.CARRYNPC: // 24
						Packet.ReadGUByte3();
						break;

					case Properties.APCOUNTER: // 25
						Packet.ReadGUByte2();
						break;

					case Properties.MAGICPOINTS: // 26
						this.Magic = Packet.ReadGUByte1();
						break;

					case Properties.KILLSCOUNT: // 27
						this.Kills = Packet.ReadGByte3();
						break;

					case Properties.DEATHSCOUNT: // 28
						this.Deaths = Packet.ReadGByte3();
						break;

					case Properties.ONLINESECS: // 29
						this.OnlineTime = Packet.ReadGByte3();
						break;

					case Properties.IPADDRESS: // 30
						this.IPAddress = new System.Net.IPAddress(Packet.ReadGUByte5()).ToString();
						break;

					case Properties.UDPPORT: // 31
						Packet.ReadGByte3();
						break;

					case Properties.ALIGNMENT: // 32
						Packet.ReadGUByte1();
						break;

					case Properties.ADDITFLAGS: // 33
						Packet.ReadGUByte1();
						break;

					case Properties.ACCOUNTNAME: // 34
						this.Account = Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.BODYIMAGE: // 35
						this.BodyImage = Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.PLRATING: // 36
						Packet.ReadGUByte3();
						break;

					case Properties.GATTRIB1: // 37
					case Properties.GATTRIB2: // 38
					case Properties.GATTRIB3: // 39
					case Properties.GATTRIB4: // 40
					case Properties.GATTRIB5: // 41
						Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.PLATTACHNPC: // 42
						Packet.ReadGUByte4();
						break;

					case Properties.GMAPLEVELX: // 43
						GmapX = Packet.ReadGUByte1();
						break;

					case Properties.GMAPLEVELY: // 44
						GmapY = Packet.ReadGUByte1();
						break;

					case Properties.PLAYERZ: // 45
						Packet.ReadGUByte1();
						break;

					case Properties.GATTRIB6: // 46
					case Properties.GATTRIB7: // 47
					case Properties.GATTRIB8: // 48
					case Properties.GATTRIB9: // 49
						Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.JOINLEAVELV: // 50
						Packet.ReadGUByte1();
						break;

					case Properties.CONNECTED: // 51
						Server.PlayerManager.DeletePlayer(Id);
						// disconnect player.
						break;

					case Properties.LANGUAGE: // 52
						Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.STATUSMSG: // 53
						Packet.ReadGUByte1();
						break;

					case Properties.GATTRIB10: // 54
					case Properties.GATTRIB11: // 55
					case Properties.GATTRIB12: // 56
					case Properties.GATTRIB13: // 57
					case Properties.GATTRIB14: // 58
					case Properties.GATTRIB15: // 59
					case Properties.GATTRIB16: // 60
					case Properties.GATTRIB17: // 61
					case Properties.GATTRIB18: // 62
					case Properties.GATTRIB19: // 63
					case Properties.GATTRIB20: // 64
					case Properties.GATTRIB21: // 65
					case Properties.GATTRIB22: // 66
					case Properties.GATTRIB23: // 67
					case Properties.GATTRIB24: // 68
					case Properties.GATTRIB25: // 69
					case Properties.GATTRIB26: // 70
					case Properties.GATTRIB27: // 71
					case Properties.GATTRIB28: // 72
					case Properties.GATTRIB29: // 73
					case Properties.GATTRIB30: // 74
						Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.OSTYPE: // 75
						Packet.ReadChars(Packet.ReadGUByte1());
						break;

					case Properties.TEXTCODEPG: // 76
						Packet.ReadGUByte3();
						break;

					case Properties.UNKNOWN77: // 77
						break;

					case Properties.PIXELX: // 78
					{
						int tmp = this.PixelX = Packet.ReadGUByte2();

						// If the first bit is 1, our position is negative.
						this.PixelX >>= 1;
						if ((tmp & 0x0001) != 0)
							this.PixelX = -this.PixelX;
						moved = true;
						break;
					}

					case Properties.PIXELY: // 79
					{
						int tmp = this.PixelY = Packet.ReadGUByte2();

						// If the first bit is 1, our position is negative.
						this.PixelY >>= 1;
						if ((tmp & 0x0001) != 0)
							this.PixelY = -this.PixelY;
						moved = true;
						break;
					}

					case Properties.PIXELZ: // 80
						Packet.ReadGUByte2();
						break;

					case Properties.UNKNOWN81: // 81
						break;

					case Properties.COMMUNITY: // 82
						Packet.ReadChars(Packet.ReadGUByte1());
						break;

					default:
						System.Windows.Forms.MessageBox.Show("Invalid Prop: " + PropId);
						break;
				}
			}

			if (moved && PlayerLevel != null)
			{
				//bool onGmap = PlayerLevel.Name.EndsWith(".gmap");
				GraalLevelNPC npc = PlayerLevel.isOnNPC(PixelX + touchtestd[dir*2], PixelY + touchtestd[dir*2+1]);
				if (npc != null)
					npc.Call("onPlayerTouchsMe", new object[] { this });
			}
		}


		/// <summary>
		/// Function -> Add Weapon to Player
		/// </summary>
		public void AddWeapon(String WeaponName)
		{
			ServerWeapon Weapon = Server.FindWeapon(WeaponName);
			if (Weapon != null)
			{
				Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLADDWEP + (short)Id + WeaponName);
				WeaponList.Add(Weapon);
			}
		}

		/// <summary>
		/// Function -> Delete Weapon from Player
		/// </summary>
		public void DeleteWeapon(String WeaponName)
		{
			DeleteWeapon(WeaponName, true);
		}

		/// <summary>
		/// Function -> Delete Weapon from Player
		/// </summary>
		public void DeleteWeapon(String WeaponName, bool SendToServer)
		{
			ServerWeapon weapon = FindWeapon(WeaponName);
			if (weapon != null)
			{
				if (SendToServer)
					Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLDELWEP + (short)Id + WeaponName);
				WeaponList.Remove(weapon);
			}
		}

		/// <summary>
		/// Function -> Find Weapon
		/// </summary>
		public ServerWeapon FindWeapon(String WeaponName)
		{
			foreach (ServerWeapon weapon in WeaponList)
			{
				if (weapon.Name == WeaponName)
					return weapon;
			}
			return null;
		}

		/// <summary>
		/// Function -> Disable Player Weapons
		/// </summary>
		public void DisableWeapons()
		{
			if ((this.PlayerStatus & (int)Status.ALLOWWEAPONS) != 0)
			{
				this.PlayerStatus &= (int)~Status.ALLOWWEAPONS;
				Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLSETSTATUS + (short)id + (byte)1 + (byte)Status.ALLOWWEAPONS);
			}
		}

		/// <summary>
		/// Function -> Enable Player Weapons
		/// </summary>
		public void EnableWeapons()
		{
			if ((this.PlayerStatus & (int)Status.ALLOWWEAPONS) == 0)
			{
				this.PlayerStatus |= (int)Status.ALLOWWEAPONS;
				Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLSETSTATUS + (short)id + (byte)0 + (byte)Status.ALLOWWEAPONS);
			}
		}

		/// <summary>
		/// Function -> SetAni (Set Animation + Gani Attributes)
		/// </summary>
		public void SetAni(String Ani, params string[] Params)
		{
			this.ani = Ani;
			// set ganiattr params
		}

		/// <summary>
		/// Function -> Send say2 Sign
		/// </summary>
		/// <param name="Message"></param>
		public void Say2(String Message)
		{
			Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLMSGSIGN + (short)Id + Message);
		}

		/// <summary>
		/// Function -> Send rpgmessage to player
		/// </summary>
		/// <param name="Message"></param>
		public void SendRpgMessage(String Message)
		{
			Server.SendGSPacket(new CString() + (byte)GServerConnection.PacketOut.NCQUERY + (byte)GServerConnection.NCREQ.PLRPGMSG + (short)Id + CString.tokenize(Message));
		}

		/// <summary>
		/// Send PM from NPC-Server
		/// </summary>
		public void SendPM(String Message)
		{
			Server.SendPM(this.Id, Message, false);
		}

		/// <summary>
		/// Functions -> Retrieve variables for scripting / update props
		/// </summary>
		public string account
		{
			get { return Account; }
		}

		public string ani
		{
			get { return Ani; }
			set { Ani = value; this.SendProp(Properties.ANIMATION); }
		}

		public int ap
		{
			get { return Ap; }
			set { Ap = value; this.SendProp(Properties.ALIGNMENT); }
		}

		public int arrows
		{
			get { return Arrows; }
			set { if (Arrows != value) { Arrows = value; this.SendProp(Properties.ARROWSCOUNT); } }
		}

		public string body
		{
			get { return BodyImage; }
			set { BodyImage = value; this.SendProp(Properties.BODYIMAGE); }
		}

		public string bodyimg
		{
			get { return BodyImage; }
			set { BodyImage = value; this.SendProp(Properties.BODYIMAGE); }
		}

		public int bombs
		{
			get { return Bombs; }
			set { if (Bombs != value) { Bombs = value; this.SendProp(Properties.BOMBSCOUNT); } }
		}

		public string chat
		{
			get { return Chat; }
			set { Chat = value; this.SendProp(Properties.CURCHAT); }
		}

		public ColorManager colors
		{
			get { return ColorManager; }
		}

		public int darts
		{
			get { return Arrows; }
			set { if (Arrows != value) { Arrows = value; this.SendProp(Properties.ARROWSCOUNT); } }
		}

		public int deaths
		{
			get { return Deaths; }
			set { Deaths = value; this.SendProp(Properties.DEATHSCOUNT); }
		}

		public int dir
		{
			get { return Dir; }
			set { Dir = value; this.SendProp(Properties.PLSPRITE); }
		}

		public FlagManager flags
		{
			get { return FlagManager; }
		}

		public int fullhearts
		{
			get { return FullHearts; }
			set { FullHearts = value; this.SendProp(Properties.MAXPOWER); }
		}

		public int glovepower
		{
			get { return GlovePower; }
			set { GlovePower = value; this.SendProp(Properties.GLOVEPOWER); }
		}

		public string guild
		{
			get { return Guild; }
			set { SetGuild(value); this.SendProp(Properties.NICKNAME); }
		}

		public int gralats
		{
			get { return Gralats; }
			set { Gralats = value; this.SendProp(Properties.RUPEESCOUNT); }
		}

		public string head
		{
			get { return HeadImage; }
			set { HeadImage = value; this.SendProp(Properties.HEADIMAGE); }
		}

		public string headimg
		{
			get { return HeadImage; }
			set { HeadImage = value; this.SendProp(Properties.HEADIMAGE); }
		}

		public double hearts
		{
			get { return Hearts; }
			set { Hearts = value; this.SendProp(Properties.CURPOWER); }
		}

		public int id
		{
			get { return Id; }
		}

		public int kills
		{
			get { return Kills; }
			set { Kills = value; this.SendProp(Properties.KILLSCOUNT); }
		}

		public string level // should return levelobject
		{
			get { return Level; }
			//set { Level = value; this.SendProp(Properties.CURLEVEL); }
		}

		public int mp
		{
			get { return Magic; }
			set { Magic = value; this.SendProp(Properties.MAGICPOINTS); }
		}

		public string nickname
		{
			get { return Nickname; }
			set { SetNick(value); this.SendProp(Properties.NICKNAME); }
		}

		public int onlinetime
		{
			get { return OnlineTime; }
		}

		public int rupees
		{
			get { return Gralats; }
			set { Gralats = value; this.SendProp(Properties.RUPEESCOUNT); }
		}

		public string shield
		{
			get { return ShieldImage; }
			set { ShieldImage = value; this.SendProp(Properties.SHIELDPOWER); }
		}

		public string shieldimg
		{
			get { return ShieldImage; }
			set { ShieldImage = value; this.SendProp(Properties.SHIELDPOWER); }
		}

		public int shieldpower
		{
			get { return ShieldPower; }
			set { ShieldPower = value; this.SendProp(Properties.SHIELDPOWER); }
		}

		public string sword
		{
			get { return SwordImage; }
			set { SwordImage = value; this.SendProp(Properties.SWORDPOWER); }
		}

		public string swordimg
		{
			get { return SwordImage; }
			set { SwordImage = value; this.SendProp(Properties.SWORDPOWER); }
		}

		public int swordpower
		{
			get { return SwordPower; }
			set { SwordPower = value; this.SendProp(Properties.SWORDPOWER); }
		}

		public bool weaponsenabled
		{
			get { return (this.PlayerStatus & (int)Status.ALLOWWEAPONS) != 0; }
		}

		public double x
		{
			get { return PixelX / 16; }
			set { PixelX = Convert.ToInt32(value * 16.0); this.SendProp(Properties.PIXELX); }
		}

		public double y
		{
			get { return PixelY / 16; }
			set { PixelY = Convert.ToInt32(value * 16.0); this.SendProp(Properties.PIXELY); }
		}
	}
}

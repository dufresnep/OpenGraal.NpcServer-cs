using System;
using System.Collections.Generic;

namespace OpenGraal.NpcServer
{
	public class AppSettings : OpenGraal.Core.IniFile
	{
		private static AppSettings _instance = new AppSettings();
		public static AppSettings GetInstance()
		{
			return _instance;
		}

		private AppSettings() : base()
		{
		}
		private string _account = null;
		public string Account
		{
			get
			{
				this.Account = this.GetValue("credentials", "account", "(npcserver)");
				
				return this._account;
			}
			set
			{
				this._account = value;

				this.SetValue("credentials", "account", this._account);
				this.Save();

			}
		}
		private string _password = null;
		public string Password
		{
			get
			{
				this.Password = this.GetValue("credentials", "password", "npcserverpass");

				return this._password;
			}
			set
			{
				this._password = value;
				this.SetValue("credentials", "password", this._password);

				this.Save();
			}
		}
		private string _nickname = null;
		public string Nickname
		{
			get
			{
				this.Nickname = this.GetValue("credentials", "nickname", "NPC (Server)");
				return this._nickname;
			}
			set
			{
				this._nickname = value;

				this.SetValue("credentials", "nickname", this._nickname);
				this.Save();

			}
		}

		private string _address = "";
		public string Address
		{
			get
			{
				this.Address = this.GetValue("connection", "address", "127.0.0.1");

				return this._address;
			}
			set
			{
				this._address =  value;

				this.SetValue("connection", "address", this._address);
				this.Save();

			}
		}

		private int _port;
		public int Port
		{
			get
			{
				this.Port = this.GetValueInt("connection", "port", 14805);
				return this._port;
			}
			set
			{
				this._port = value;

				this.SetValue("connection", "port", this._port.ToString());
				this.Save();

			}
		}
		
		private int _ncPort;
		public int NCPort
		{
			get
			{
				this.NCPort = this.GetValueInt("connection", "ncport", 14855);
				return this._ncPort;
			}
			set
			{
				this._ncPort = value;

				this.SetValue("connection", "ncport", this._ncPort.ToString());
				this.Save();

			}
		}
	}
}
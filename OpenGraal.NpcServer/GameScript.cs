using System;
using System.Collections.Generic;
using System.Reflection;
using OpenGraal.NpcServer;

namespace OpenGraal.NpcServer
{
	public class ScriptEvent
	{
		/// <summary>
		/// Member Variables
		/// </summary>
		public double FunctionTimer = 0.0;
		public object[] FunctionArgs;
		public string FunctionName;

		/// <summary>
		/// Constructor - Create Event
		/// </summary>
		public ScriptEvent(double Timeout, String Name, Object[] Args)
		{
			this.FunctionTimer = Timeout;
			this.FunctionName = Name;
			this.FunctionArgs = Args;
		}
	};

	/// <summary>
	/// Reference Interface
	/// </summary>
	public class IRefObject
	{
		/// <summary>
		/// Script Type
		/// </summary>
		public enum ScriptType
		{
			WEAPON = 0,
			LEVELNPC = 1,
		}

		/// <summary>
		/// Member Variables
		/// </summary>
		public ScriptObj ScriptObject;
		public ScriptType Type;
		public String Script;

		/// <summary>
		/// Constructor
		/// </summary>
		public IRefObject(ScriptType Type)
		{
			this.Type = Type;
		}

		/// <summary>
		/// Call Function
		/// </summary>
		public void Call(string Event, object[] Args)
		{
			if (ScriptObject == null)
				return;

			try
			{
				ScriptObject.Call(Event, Args);
			}
			catch (TargetInvocationException e)
			{
				ScriptObject.SendToNC("Script runtime error occurred:\rerror: " + e.InnerException.Message);
			}
		}
	}

	public static class ExtensionMethods
	{
		/// <summary>
		/// Extension Method -> ToInt
		/// </summary>
		public static int ToInt(this String str)
		{
			try
			{
				return Convert.ToInt32(str);
			}
			catch (System.FormatException)
			{
				return 0;
			}
		}

		/// <summary>
		/// Extension Method -> ToInt
		/// </summary>
		public static double ToDouble(this String str)
		{
			try
			{
				return Convert.ToDouble(str);
			}
			catch (System.FormatException)
			{
				return 0;
			}
		}

		/// <summary>
		/// Extension Method -> Case-insensitive compare
		/// </summary>
		public static bool comparei(this String str, String str2)
		{
			return string.Equals(str, str2, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Extension Method -> Case-insensitive compare
		/// </summary>
		public static bool startswith(this String str, String str2)
		{
			return str.StartsWith(str2, StringComparison.OrdinalIgnoreCase);
		}
	}

	/// <summary>
	/// ScriptObject Class
	/// </summary>
	public class ScriptObj
	{
		// -- Member Variables -- //
		static internal Random rand = new Random();
		internal readonly Framework Server = null;
		internal List<ScriptEvent> ScriptEvents = new List<ScriptEvent>();
		public double timeout = 0.0;

		/// <summary>
		/// Constructor
		/// </summary>
		public ScriptObj() { }
		public ScriptObj(Framework Server)
		{
			this.Server = Server;
		}

		/// <summary>
		/// Function -> Call Event for Object
		/// </summary>
		internal void Call(String Event, Object[] Args)
		{
			Type type = this.GetType();
			MethodInfo m = type.GetMethod(Event);
			if (m != null)
				type.InvokeMember(Event, BindingFlags.InvokeMethod, null, this, Args);
		}

		/// <summary>
		/// Function -> Run Scheduled Events
		/// </summary>
		internal void RunEvents()
		{
			// Run Timeout
			if (timeout > 0.0)
			{
				timeout -= 0.05;
				if (timeout <= 0.0)
					Call("onTimeout", null);
			}

			// Run Scheduled Events
			for (int i = ScriptEvents.Count-1; i >= 0; i--)
			{
				ScriptEvent e = ScriptEvents[i];
				if (e.FunctionTimer > 0.0)
				{
					e.FunctionTimer -= 0.05;
					if (e.FunctionTimer <= 0.0)
					{
						Call(e.FunctionName, e.FunctionArgs);
						ScriptEvents.RemoveAt(i);
					}
				}
			}
		}

		/// <summary>
		/// Library Function -> FindWeapon by Name
		/// </summary>
		public dynamic FindWeapon(string Name)
		{
			ServerWeapon wep;
			if (Server.WeaponList.TryGetValue(Name, out wep))
				return (dynamic)wep.ScriptObject;
			return null;
		}

		/// <summary>
		/// Library Function -> Find Player by Account
		/// </summary>
		public GraalPlayer FindPlayer(string Account)
		{
			return Server.PlayerManager.FindPlayer(Account);
		}

		/// <summary>
		/// Library Function -> Find Player by Id
		/// </summary>
		public GraalPlayer FindPlayer(short Id)
		{
			return Server.PlayerManager.FindPlayer(Id);
		}

		/// <summary>
		/// Library Function -> MD5 String
		/// </summary>
		/// <param name="Input"></param>
		/// <returns></returns>
		public String MD5(String Input)
		{
			System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] bs = x.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Input));
			string ret = String.Empty;
			foreach (byte b in bs)
				ret += b.ToString("x2").ToLower();
			return ret;
		}

		/// <summary>
		/// Random (Integer)
		/// </summary>
		public double random(int min, int max)
		{
			return min + rand.NextDouble() * (max - min);
		}

		/// <summary>
		/// Library Function -> Schedule Event
		/// </summary>
		public void ScheduleEvent(double Timer, String Name, params object[] Args)
		{
			ScriptEvents.Add(new ScriptEvent(Timer, Name, Args));
		}

		/// <summary>
		/// Library Function -> Trigger Event
		/// </summary>
		public void trigger(string Event, params object[] Args)
		{
			this.Call(Event, Args);
		}

		/// <summary>
		/// Library Function -> SetTimer
		/// </summary>
		public void SetTimer(double Timer)
		{
			this.timeout = Timer;
		}

		/// <summary>
		/// Library Function -> Send Text to NPC-Controls
		/// </summary>
		public void SendToNC(String Message)
		{
			Server.SendNCChat(Message, null);
		}

		/// <summary>
		/// Library Function -> Send Text to Remote Controls
		/// </summary>
		public void SendToRC(String Message)
		{
			Server.SendRCChat(Message);
		}

		/// <summary>
		/// Retrieve Players-Array
		/// </summary>
		public GraalPlayerList allplayers
		{
			get { return Server.PlayerManager; }
		}

		/// <summary>
		/// New World Time
		/// </summary>
		public int timevar
		{
			get { return Server.NWTime; }
		}

		/// <summary>
		/// Computer Time
		/// </summary>
		public double timevar2
		{
			get { return (double)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds; }
		}
	};
}
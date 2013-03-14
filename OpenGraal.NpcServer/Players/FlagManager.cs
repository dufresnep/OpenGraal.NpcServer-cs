using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGraal.NpcServer.Players
{
	public class FlagManager
	{
		/// <summary>
		/// Member Variables
		/// </summary>
		internal Dictionary<string, string> FlagList = new Dictionary<string, string>();
		internal dSendFlag cFunction = null;
		internal delegate void dSendFlag(string key, string val);
		
		/// <summary>
		/// Constructor
		/// </summary>
		internal FlagManager(dSendFlag Delegate)
		{
			this.cFunction = Delegate;
		}

		/// <summary>
		/// Retrieve / Send Flag
		/// </summary>
		public string this[string key]
		{
			get
			{
				String FlagValue;
				if (FlagList.TryGetValue(key, out FlagValue))
					return FlagValue;
				return String.Empty;
			}

			set
			{
				FlagList[key] = value;
				if (cFunction != null)
					cFunction(key, value);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGraal.NpcServer
{
	public class ServerClass
	{
		/// <summary>
		/// Member Variables
		/// </summary>
		public String Name, Script;

		/// <summary>
		/// Constructor
		/// </summary>
		internal ServerClass(String ClassName, String ClassScript)
		{
			this.UpdateClass(ClassName, ClassScript);
		}

		/// <summary>
		/// Update Weapon Properties
		/// </summary>
		internal void UpdateClass(String ClassName, String ClassScript)
		{
			this.Name = ClassName;
			this.Script = ClassScript;
		}
	}
}

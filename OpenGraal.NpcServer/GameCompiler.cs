using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CSharp;

namespace OpenGraal.NpcServer
{
	public class GameCompiler
	{
		/// <summary>
		/// Compile Thread
		/// </summary>
		public void CompileThread()
		{
			while (true)
			{
				while (CompileList.Count > 0)
				{
					// Grab from Queue
					IRefObject cur;
					lock (CompileList)
						cur = CompileList.Dequeue();

					// Compile
					this.Compile(cur);

					// Add to Run List
					lock (RunList)
						RunList.Enqueue(cur);
				}

				Thread.Sleep(50);
			}
		}

		/// <summary>
		/// Member Variables
		/// </summary>
		protected Framework Server;
		protected Int32[] NextId = new Int32[] { 0, 0 };
		protected Queue<IRefObject> CompileList = new Queue<IRefObject>();
		internal Queue<IRefObject> RunList = new Queue<IRefObject>(); 
		
		/// <summary>
		/// Constructor -> Create Compiler, pass NPCServer reference
		/// </summary>
		internal GameCompiler(Framework Server)
		{
			this.Server = Server;
			Thread th = new Thread(CompileThread);
			th.IsBackground = true;
			th.Start();

			Thread th2 = new Thread(CompileThread);
			th2.IsBackground = true;
			th2.Start();
		}

		/// <summary>
		/// Add to Compile List
		/// </summary>
		public void CompileAdd(IRefObject ScriptObj)
		{
			lock (CompileList)
				CompileList.Enqueue(ScriptObj);
		}

		/// <summary>
		/// Compile & Execute -> Script
		/// </summary>
		/// <returns></returns>
		public void Compile(IRefObject ScriptObj)
		{
			CompilerResults Results;
			Assembly Asm = CompileScript(ScriptObj, out Results);
			if (Asm != null)
				ScriptObj.ScriptObject = this.RunScript(ScriptObj, Asm);
			else
				HandleErrors((ScriptObj.Type == IRefObject.ScriptType.WEAPON ? "weapon" + ((ServerWeapon)ScriptObj).Name : "levelnpc" + ((GraalLevelNPC)ScriptObj).Id), Results);
		}

		/// <summary>
		/// Compile Script -> Return Assembly
		/// </summary>
		public Assembly CompileScript(IRefObject ScriptObject, out CompilerResults Result)
		{
			String ClassName, PrefixName;
			switch (ScriptObject.Type)
			{
				case IRefObject.ScriptType.WEAPON:
					ClassName = "ScriptWeapon";
					PrefixName = "WeaponPrefix";
					break;

				default:
					ClassName = "ScriptLevelNpc";
					PrefixName = "LevelNpcPrefix";
					break;
			}

			// Setup our options
			CompilerParameters options = new CompilerParameters();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
			options.ReferencedAssemblies.Add("System.dll");
			options.ReferencedAssemblies.Add("System.Core.dll");
			options.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

			// Compile our code
			CSharpCodeProvider csProvider = new Microsoft.CSharp.CSharpCodeProvider();
			Result = csProvider.CompileAssemblyFromSource(options, "using CS_NPCServer; public class " + PrefixName + NextId[(int)ScriptObject.Type] + " : " + ClassName + " { public " + PrefixName + NextId[(int)ScriptObject.Type] + "(NPCServer Server, IRefObject Ref) : base(Server, Ref) { } " + ParseJoins(ScriptObject.Script) + " } ");
			NextId[(int)ScriptObject.Type]++;
			return (Result.Errors.HasErrors ? null : Result.CompiledAssembly);
		}

		/// <summary>
		/// Send Errors to NC Chat
		/// </summary>
		public void HandleErrors(String Name, CompilerResults Results)
		{
			if (Results.Errors.HasErrors)
			{
				Server.SendNCChat("Script compiler output for " + Name + ":");
				foreach (CompilerError CompErr in Results.Errors)
					Server.SendNCChat("error: " + CompErr.ErrorText + " at line " + CompErr.Line);
			}
		}

		/// <summary>
		/// Parse Joins and return new script
		/// </summary>
		public string ParseJoins(String Script)
		{
			MatchCollection col = Regex.Matches(Script, "join\\(\"(?<class>[A-Za-z0-9]*)\"\\);", RegexOptions.IgnoreCase);
			String NewScript = Regex.Replace(Script, "join\\(\"(?<class>[A-Za-z0-9]*)\"\\);", "", RegexOptions.IgnoreCase);
			foreach (Match x in col)
			{
				ServerClass Class = Server.FindClass(x.Groups["class"].Value);
				if (Class != null)
					NewScript += "\n" + Class.Script;
			}

			return NewScript;
		}

		/// <summary>
		/// Run Script -> Return Object
		/// </summary>
		public ScriptObj RunScript(IRefObject Reference, Assembly Script)
		{
			Type[] types = Script.GetTypes();
			foreach (Type type in types)
			{
				if (!type.IsSubclassOf(typeof(ScriptObj)))
					continue;

				ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(Framework), typeof(IRefObject) });
				if (constructor != null && constructor.IsPublic)
				{
					ScriptObj obj = (ScriptObj)constructor.Invoke(new object[] { Server, Reference });
					return obj;
				}
			}

			// No object created, return null
			return null;
		}
	}
}

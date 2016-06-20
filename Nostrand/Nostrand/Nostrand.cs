﻿using System;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using clojure.lang;
using Mono.Terminal;

namespace Nostrand
{
	public class Nostrand
	{

		[DllImport("__Internal", EntryPoint="mono_get_runtime_build_info")]
		public extern static string GetMonoVersion();

		public static string Version()
		{
			var asm = Assembly.GetCallingAssembly();
			return asm.GetName().Version + " " + asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		}

		public static void Main(string[] args)
		{
			Terminal.Message("Nostrand", Version());
			Terminal.Message("Mono", GetMonoVersion());
			Console.WriteLine();
			// Terminal.Message ("Clojure", RT.var("clojure.core", "clojure-version").invoke());

			if(args.Length > 0) {
				var task = args[0];
				var taskParts = task.Split('/');
				var taskNS = taskParts[0];
				var taskVar = taskParts[1];
				RT.load(taskNS.Replace('.', '/'));
				RT.var(taskNS, taskVar).invoke();
			}

			LineEditor le = new LineEditor("nostrand");
			le.AutoCompleteEvent += delegate (string a, int pos)
			{
				string prefix = "";
				var completions = new string[] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };
				return new Mono.Terminal.LineEditor.Completion(prefix, completions);
			};

			string s;

			new Thread(() => RT.load("clojure/core")).Start();

			while ((s = le.Edit("clojure.core> ", "")) != null)
			{
				try
				{
					var readResult = RT.var("clojure.core", "read-string").invoke(s);
					var evaledResult = RT.var("clojure.core", "eval").invoke(readResult);
					var stringResult = RT.var("clojure.core", "pr-str").invoke(evaledResult).ToString();
					Terminal.Message(stringResult, ConsoleColor.Gray);
				}
				catch (System.IO.EndOfStreamException)
				{

				}
				catch (Exception e)
				{
					Terminal.Message("Exception", e.ToString(), ConsoleColor.Yellow);
				}
			}
		}
	}
}


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeaseAI_CE.Scripting
{
	public class BlockScope
	{
		public readonly Controller Controller;
		public readonly BlockBase Root;
		public readonly Line Block;

		/// <summary> Current line in Block.Lines </summary>
		public int Line;
		/// <summary> If true, repeat current line and block. </summary>
		public bool Repeat = false;
		/// <summary> Vairables local to this scope. </summary>
		public Dictionary<string, ValueObj> Variables;
		/// <summary> Results of last if statement, could easily just be a local variable. </summary>
		public bool LastIf = false;
		/// <summary> If true clear stack, and exit this scope. </summary>
		public bool Exit = false;
		/// <summary> If true exit just this scope </summary>
		public bool Return = false;

		public BlockScope(Controller controller, BlockBase root, Line block, int line, Dictionary<string, ValueObj> variables)
		{
			Controller = controller;
			Root = root;
			Block = block;
			Line = line;
			Variables = variables;
		}

		public ValueObj GetVariable(string key)
		{
			if (key.StartsWith(".local"))
			{
				ValueObj result;
				if (!Variables.TryGetValue(key, out result))
					Root.Log.Error("Variable does not exist in the current scope: " + key);
				return result;
			}
			else
				return Controller.Personality.GetVariable(key, Root.Log);
		}
	}
}
using System;

namespace LegendDrive
{

	public class NumpadButton
	{
		public NumpadButton(string text, int x, int y, NumpadCommands cmd, Action<object, NumpadCommands> action)
		{
			Text = text;
			X = x;
			Y = y;
			Command = cmd;
			ExecuteCommand = action;
		}

		public string Text { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }
		public NumpadCommands Command { get; private set; }
		public Action<object, NumpadCommands> ExecuteCommand { get; private set; } 
	}
	
}

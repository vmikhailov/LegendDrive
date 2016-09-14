using System;
using System.Linq;
using System.Collections.Generic;

namespace LegendDrive
{
	public class NumpadConfiguration
	{
		public NumpadConfiguration(int x, int y)
		{
			X = x;
			Y = y;
		}

		public int X { get; set; }
		public int Y { get; set; }

		private List<NumpadButton> _buttons = new List<NumpadButton>();
		public IEnumerable<NumpadButton> Buttons { get { return _buttons;} }

		public void Add(string text, int x, int y, NumpadCommands cmd, Action<object, NumpadCommands> action)
		{
			if (x >= 0 && x < X && y >= 0 && y < Y)
			{
				if (!_buttons.Where(z => z.X == x && z.Y == y).Any())
				{
					_buttons.Add(new NumpadButton(text, x, y, cmd, action));
					return;
				}	
			}
			throw new Exception("Duplicate key defintion for numpad");
		}

		public void Add(string text, int x, int y, int cmd, Action<object, NumpadCommands> action)
		{
			Add(text, x, y, (NumpadCommands)cmd, action);
		}

	}
}


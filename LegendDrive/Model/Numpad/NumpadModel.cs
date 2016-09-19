using System;
using System.ComponentModel;
using System.Linq;
using LegendDrive.Counters;

namespace LegendDrive
{
	public class NumpadModel : BaseBindingObject<NumpadModel>
	{

		public event Func<string, bool> NewDataTextEntered;

		string _newDataText;
		public string NewDataText
		{
			get { return _newDataText ?? string.Empty; }
			set
			{
				if (value.Equals(_newDataText, StringComparison.Ordinal))
				{
					// Nothing to do - the value hasn't changed;
					return;
				}
				_newDataText = value;
				RaisePropertyChanged(nameof(NewDataText));
			}
		}

		public NumpadConfiguration NumpadConfiguration
		{
			get
			{
				var cfg = new NumpadConfiguration(3, 5);

				for (int i = 0; i < 9; i++)
				{
					cfg.Add($"{i + 1}", i % 3, i /3, i+1, OnButtonCommand_Digit);
				}

				cfg.Add("_", 0, 3, NumpadCommands.KeySpace, OnButtonCommand_Space);
				cfg.Add("0",  1, 3, NumpadCommands.Key0, OnButtonCommand_Digit);
				cfg.Add(".",  2, 3, NumpadCommands.KeyDot, OnButtonCommand_Digit);

				cfg.Add("C",  0, 4, NumpadCommands.KeyClear, OnButtonCommand_Clear);
				cfg.Add("<-", 1, 4, NumpadCommands.Key0, OnButtonCommand_Back);
				cfg.Add("+",  2, 4, NumpadCommands.KeyDot, OnButtonCommand_Enter);

				return cfg;
			}
		}

		private void OnButtonCommand_Digit(object sender, NumpadCommands cmd)
		{
			if (cmd == NumpadCommands.KeyDot)
			{
				if (!NewDataText.Split(' ').Last().Contains('.'))
				{
					NewDataText += ".";
				}
			}
			else
			{
				NewDataText += ((int)cmd).ToString();
			}
		}

		private void OnButtonCommand_Clear(object sender, NumpadCommands cmd)
		{
			NewDataText = String.Empty;
		}

		private void OnButtonCommand_Space(object sender, NumpadCommands cmd)
		{
			if (!NewDataText.EndsWith(" "))
			{
				NewDataText += " ";
			}
		}

		private void OnButtonCommand_Back(object sender, NumpadCommands cmd)
		{
			if (NewDataText.Length > 0)
			{
				NewDataText = NewDataText.Substring(0, NewDataText.Length - 1);
			}
		}

		private void OnButtonCommand_Enter(object sender, NumpadCommands cmd)
		{
			if (NewDataTextEntered != null && NewDataTextEntered(NewDataText))
			{
				NewDataText = string.Empty;
			};
		}

	}
}



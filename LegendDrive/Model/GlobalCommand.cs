namespace LegendDrive.Model
{
	public enum GlobalCommandCodes
	{
		StartFinish,
		ClearAll,
		ResetAll,
		Back,
		GPSReset,
		Turn,
		DelSegment,
		AskConfirmation,
		ReplyConfirmation,
		Vibrate,
		None
	}

	public enum RaceEventTypes
	{
		Start,
		Finish,
		Turn,
		Back,
		NewLocation
	}

	public class RaceEvent
	{
		public RaceEvent(LocationData loc, RaceEventTypes type, int segid = 0)
		{
			Location = loc;
			Type = type;
			SegmentId = segid;
		}

		public LocationData Location { get; private set; }

		public RaceEventTypes Type { get; private set; }

		public int SegmentId { get; private set; }
	}

	public class VibrateCommand
	{
		public VibrateCommand(string pattern)
		{
			Pattern = pattern;
		}

		public VibrateCommand(string pattern, int impulse)
		{
			Pattern = pattern;
			ImpulseLength = impulse;
		}

		public VibrateCommand(int impulse)
		{
			Pattern = "1";
			ImpulseLength = impulse;
		}


		public string Pattern { get; private set; }
		public int ImpulseLength { get; private set; } = 100;
		public int PauseLength { get; private set; } = 100;
	}



	public class GlobalCommand
	{
		public static GlobalCommand StartFinish { get; } = new GlobalCommand(GlobalCommandCodes.StartFinish);
		public static GlobalCommand ClearAll { get; } = new GlobalCommand(GlobalCommandCodes.ClearAll);
		public static GlobalCommand ResetAll { get; } = new GlobalCommand(GlobalCommandCodes.ResetAll);
		public static GlobalCommand Back { get; } = new GlobalCommand(GlobalCommandCodes.Back);
		public static GlobalCommand GPSReset { get; } = new GlobalCommand(GlobalCommandCodes.GPSReset);

		public static GlobalCommand Turn { get; } = new GlobalCommand(GlobalCommandCodes.Turn);
		public static GlobalCommand None { get; } = new GlobalCommand(GlobalCommandCodes.None);
		public static GlobalCommand DelSegment { get; } = new GlobalCommand(GlobalCommandCodes.DelSegment);
		public static GlobalCommand AskConfirmation(GlobalCommand cmdToConfirm, string message)
		{
			return new GlobalCommand(GlobalCommandCodes.AskConfirmation, cmdToConfirm, message);
		}

		public static GlobalCommand ReplyConfirmation(GlobalCommand cmdToConfirm)
		{
			return new GlobalCommand(GlobalCommandCodes.ReplyConfirmation, cmdToConfirm);
		}


		public GlobalCommand(GlobalCommandCodes code)
		{
			Code = code;
		}

		public GlobalCommand(GlobalCommandCodes code, GlobalCommand cmdToConfirm, string message = null)
		{
			Code = code;
			Message = message;
			CommandToConfirm = cmdToConfirm;
		}

		public string Message { get; }
		public GlobalCommand CommandToConfirm { get; }
		public GlobalCommandCodes Code { get; }
	}
}


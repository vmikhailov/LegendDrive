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
		ReplyConfirmation
	}

	public class GlobalCommand
	{
		public static GlobalCommand StartFinish { get; } = new GlobalCommand(GlobalCommandCodes.StartFinish);
		public static GlobalCommand ClearAll { get; } = new GlobalCommand(GlobalCommandCodes.ClearAll);
		public static GlobalCommand ResetAll { get; } = new GlobalCommand(GlobalCommandCodes.ResetAll);
		public static GlobalCommand Back { get; } = new GlobalCommand(GlobalCommandCodes.Back);
		public static GlobalCommand GPSReset { get; } = new GlobalCommand(GlobalCommandCodes.GPSReset);

		public static GlobalCommand Turn { get; } = new GlobalCommand(GlobalCommandCodes.Turn);
		public static GlobalCommand DelSegment { get; } = new GlobalCommand(GlobalCommandCodes.DelSegment);
		public static GlobalCommand AskCofirmation(GlobalCommand cmdToConfirm, string message)
		{
			return new GlobalCommand(GlobalCommandCodes.AskConfirmation, cmdToConfirm, message);
		}

		public static GlobalCommand ReplyCofirmation(GlobalCommand cmdToConfirm)
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


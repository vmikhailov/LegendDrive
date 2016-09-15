using System;
using LegendDrive.Persistance;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Model.RaceModel
{
	public class TurnInfo : ISupportStatePersistance
	{
		public int No
		{
			get; set;
		}

		public DateTime TurnTime
		{
			get; set;
		}

		public JObject GetState()
		{
			var obj = new JObject();
			obj.AddValue(nameof(No), No);
			obj.AddValue(nameof(TurnTime), TurnTime);
			return obj;
		}

		public void LoadState(JObject obj)
		{
			No = obj.GetValue<int>(nameof(No));
			TurnTime = obj.GetValue<DateTime>(nameof(TurnTime));
		}
	}
}


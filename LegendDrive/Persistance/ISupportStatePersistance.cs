using System;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Persistance
{
	public interface ISupportStatePersistance
	{
		JObject GetState();
		void LoadState(JObject obj);
	}
}

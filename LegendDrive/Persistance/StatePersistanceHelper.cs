using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LegendDrive.Persistance
{
	public static class JObjectExtension
	{
		public static void AddValue<T>(this JObject obj, string property, T value)
		{
			obj.Add(new JProperty(property, value));
		}

		public static T GetValue<T>(this JObject obj, string property)
		{
			var p = obj[property];
			return p != null ? p.ToObject<T>() : default(T);
		}

		public static void AddCollection<T>(this JObject obj, string property, ICollection<T> target)
			where T : ISupportStatePersistance
		{
			var jobjs = target.Select(x => x.GetState()).ToList();
			obj.AddValue(property, jobjs);
		}


		public static void LoadCollection<T>(this JObject obj, string property, ICollection<T> target) 
			where T: ISupportStatePersistance, new()
		{
			var jobjs = obj.GetValue<List<JObject>>(property);
			target.Clear();
			foreach (var jo in jobjs)
			{
				var t = new T();
				t.LoadState(jo);
				target.Add(t);
			}
		}
	}
}

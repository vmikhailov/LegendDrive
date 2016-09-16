using System;
namespace LegendDrive.Droid.Services
{
	public class BaseService<T> 
	{
		T _service;
		string _providerName;

		public BaseService(T service)
		{
			_service = service;
		}


	}
}

using System;
namespace LegendDrive.Model.ViewModel
{
	public interface IViewModel<T>
	{
		T Model { get; set; }
	}
}

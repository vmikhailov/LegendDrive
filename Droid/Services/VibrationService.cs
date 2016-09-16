using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using LegendDrive.Messaging;
using LegendDrive.Model;

namespace LegendDrive.Droid.Services
{
	public class VibrationService
	{
		readonly Vibrator vibrator;
		public VibrationService(Vibrator vibrationService)
		{
		    vibrator = vibrationService;
			MessagingHub.Subscribe<VibrateCommand>(this, (cmd) => ProcessCommand(cmd));
		}

		public void Vibrate(int msec)
		{
			vibrator.Vibrate(msec);
		}

		void ProcessCommand(VibrateCommand cmd)
		{
			foreach (var c in cmd.Pattern)
			{
				var d = (int)c - 48;
				if (d > 0)
				{
					Vibrate(cmd.ImpulseLength * d);
					Thread.Sleep(cmd.ImpulseLength * d);
				}
				else
				{
					Thread.Sleep(cmd.ImpulseLength);
				}
				Thread.Sleep(cmd.PauseLength);
			}
		}

		public void Init()
		{
		}
	}
}

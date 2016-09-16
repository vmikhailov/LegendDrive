using System;
using Xamarin.Forms;

namespace LegendDrive.Messaging
{
	public static class MessagingHub
	{
		public static void Send<TMessage, TArg>(QueueType queue, TMessage message, TArg arg)
			where TMessage: class
		{
			MessagingCenter.Send(message, queue.ToString(), arg);
		}

		public static void Send<TMessage>(QueueType queue, TMessage message)
			where TMessage : class
		{
			MessagingCenter.Send(message, queue.ToString());
		}

		public static void Send<TMessage>(TMessage message)
			where TMessage : class
		{
			MessagingCenter.Send(message, typeof(TMessage).Name);
		}

		public static void Subscribe<TMessage, TArg>(object subscriber, QueueType queue, Action<TMessage, TArg> action)
			where TMessage : class
		{
			MessagingCenter.Subscribe(subscriber, queue.ToString(), action);
		}

		public static void Subscribe<TMessage>(object subscriber, QueueType queue, Action<TMessage> action)
			where TMessage : class
		{
			MessagingCenter.Subscribe(subscriber, queue.ToString(), action);
		}

		public static void Subscribe<TMessage>(object subscriber, Action<TMessage> action)
			where TMessage : class
		{
			MessagingCenter.Subscribe(subscriber, typeof(TMessage).Name, action);
		}
	}

	public enum QueueType
	{
		Global,
		Click,
		Location,
		AskConfirmation,
		Confirmed,
		Canceled,
		Gesture,
		Race
	}
}

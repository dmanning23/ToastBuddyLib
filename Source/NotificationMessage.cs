using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontBuddyLib;

namespace ToastBuddyLib
{
	/// <summary>
	/// Helper class stores the position and text of a single notification message.
	/// </summary>
	internal class NotificationMessage
	{
		/// <summary>
		/// The text of this message
		/// </summary>
		public string TextMessage { get; private set; }

		/// <summary>
		/// The screen position of this message
		/// </summary>
		public float Position { get; set; }

		/// <summary>
		/// how old is this message
		/// </summary>
		public TimeSpan Age;

		/// <summary>
		/// Initializes a new instance of the <see cref="ToastBuddyLib.NotificationMessage"/> class.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="yPosition">Y position.</param>
		public NotificationMessage(string text, float yPosition)
		{
			TextMessage = text;
			Position = yPosition;
			Age = TimeSpan.Zero;
		}
	}
}
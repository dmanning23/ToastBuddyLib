using System;
using Microsoft.Xna.Framework;

namespace ToastBuddyLib
{
	/// <summary>
	/// Helper class stores the position and text of a single notification message.
	/// </summary>
	internal class ToastMessage
	{
		#region Properties

		/// <summary>
		/// how old is this message
		/// </summary>
		public TimeSpan Age;

		/// <summary>
		/// The color to write this message
		/// </summary>
		public Color Color { get; private set; }

		/// <summary>
		/// The text of this message
		/// </summary>
		public string TextMessage { get; private set; }

		/// <summary>
		/// The screen position of this message
		/// </summary>
		public float Position { get; set; }

		#endregion //Properties

		/// <summary>
		/// Initializes a new instance of the <see cref="ToastBuddyLib.ToastMessage"/> class.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="yPosition">Y position.</param>
		/// <param name="color"></param>
		public ToastMessage(string text, float yPosition, Color color)
		{
			TextMessage = text;
			Position = yPosition;
			Age = TimeSpan.Zero;
			Color = color;
		}
	}
}
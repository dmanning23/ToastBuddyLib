using GameTimer;
using Microsoft.Xna.Framework;

namespace ToastBuddyLib
{
	/// <summary>
	/// Helper class stores the position and text of a single notification message.
	/// </summary>
	public class ToastMessage
	{
		#region Properties

		/// <summary>
		/// how old is this message
		/// </summary>
		public GameClock StateTimer { get; set; }

		public ToastMessageState State { get; set; }

		/// <summary>
		/// The color to write this message
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// The text of this message
		/// </summary>
		public string TextMessage { get; set; }

		/// <summary>
		/// The screen position of this message
		/// </summary>
		public float Position { get; set; }

		public float Scale { get; set; }

		#endregion //Properties

		/// <summary>
		/// Initializes a new instance of the <see cref="ToastBuddyLib.ToastMessage"/> class.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="yPosition">Y position.</param>
		/// <param name="color"></param>
		public ToastMessage(string text, float yPosition, Color color, float scale)
		{
			TextMessage = text;
			Position = yPosition;
			Color = color;
			Scale = scale;

			State = ToastMessageState.FadingIn;
			StateTimer = new GameClock();
			StateTimer.Start();
		}

		public void Update(GameClock time)
		{
			StateTimer.Update(time);
		}
	}
}
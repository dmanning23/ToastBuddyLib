using FontBuddyLib;
using GameTimer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ToastBuddyLib
{
	/// <summary>
	/// Component implements the IToastBuddy interface. 
	/// This is used to show notification messages when interesting events occur, 
	/// for instance when gamers join or leave the network session
	/// </summary>
	public abstract class BaseToastBuddy : IToastBuddy
	{
		#region Properties

		/// <summary>
		/// deafult amount of time to fade in messages
		/// </summary>
		private readonly float _defaultFadeInTime = 0.25f;

		/// <summary>
		/// default amount of time to fade out messages
		/// </summary>
		private readonly float _defaultFadeOutTime = 0.5f;

		/// <summary>
		/// The location to try to show toast notifications at.  
		/// Messages will queue up underneath as more are added.
		/// </summary>
		private readonly PositionDelegate DisplayPosition;

		/// <summary>
		/// The name of the font to use.
		/// </summary>
		private readonly string FontName;

		/// <summary>
		/// A callback function used to get a matrix to scale/rotate toast messages
		/// </summary>
		private readonly MatrixDelegate GetMatrix;

		/// <summary>
		/// List of the currently visible notification messages.
		/// </summary>
		protected List<ToastMessage> Messages { get; set; } = new List<ToastMessage>();

		/// <summary>
		/// Coordinates threadsafe access to the message list.
		/// </summary>
		private readonly object _lock = new object();

		private bool UseFontPlus { get; set; }

		private int FontSize { get; set; }

		/// <summary>
		/// The font helper used to write the text with a little shadow
		/// </summary>
		private ShadowTextBuddy FontHelper { get; set; }

		/// <summary>
		/// The sprite batch used to write messages
		/// </summary>
		private SpriteBatch spriteBatch { get; set; }

		private Justify Justify { get; set; }

		protected GameClock Time { get; set; }

		// Tweakable settings control how long each message is visible.
		public float FadeInTime { get; set; }
		public float FadeOutTime { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Constructs a new message display component.
		/// </summary>
		public BaseToastBuddy(string fontResource,
			PositionDelegate messagePosition,
			MatrixDelegate getMatrixDelegate,
			Justify justify = Justify.Right,
			bool useFontPlus = false,
			int fontSize = 48) : base()
		{
			//grab those other items
			FontName = fontResource;
			DisplayPosition = messagePosition;
			GetMatrix = getMatrixDelegate;
			Justify = justify;
			UseFontPlus = useFontPlus;
			FontSize = fontSize;

			FadeInTime = _defaultFadeInTime;
			FadeOutTime = _defaultFadeOutTime;

			Time = new GameClock();
			Time.Start();
		}

		/// <summary>
		/// Load graphics content for the message display.
		/// </summary>
		public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
		{
			spriteBatch = new SpriteBatch(graphicsDevice);
			FontHelper = new ShadowTextBuddy
			{
				ShadowOffset = new Vector2(0.0f, 3.0f),
				ShadowSize = 1.0f,
			};

			FontHelper.LoadContent(content, FontName, UseFontPlus, FontSize);
		}

		/// <summary>
		/// Updates the message display component.
		/// </summary>
		public void Update(GameTime gameTime)
		{
			lock (_lock)
			{
				Time.Update(gameTime);

				//Every message wants to be the first one in line.
				float targetPosition = 0;

				// Update each message in turn.
				int index = 0;
				while (index < Messages.Count)
				{
					var message = Messages[index];

					// Gradually slide the message toward its desired position.
					var positionDelta = targetPosition - message.Position;
					var velocity = (float)gameTime.ElapsedGameTime.TotalSeconds * 2;
					message.Position += positionDelta * Math.Min(velocity, 1);

					// Update the age of the message.
					UpdateMessage(message);

					if (message.State == ToastMessageState.FadingIn || message.State == ToastMessageState.Showing)
					{
						// This message is still alive.
						index++;
						targetPosition++;
					}
					else if (message.State == ToastMessageState.FadingOut)
					{
						index++;
					}
					else if (message.State == ToastMessageState.Dead)
					{
						// This message is old, and should be removed.
						Messages.RemoveAt(index);
					}
				}
			}
		}

		protected abstract void UpdateMessage(ToastMessage message);

		/// <summary>
		/// Draws the message display component.
		/// </summary>
		public void Draw(GameTime gameTime)
		{
			lock (_lock)
			{
				// Early out if there are no messages to display.
				if (Messages.Count == 0)
				{
					return;
				}

				//The start position that messages will be displayed at!
				var startPos = ((null != DisplayPosition) ? DisplayPosition() : Vector2.Zero);
				var currentMessagePosition = startPos;

				spriteBatch.Begin(SpriteSortMode.Deferred,
								  BlendState.NonPremultiplied,
								  null,
								  null,
								  null,
								  null,
								  ((null != GetMatrix) ? GetMatrix() : Matrix.Identity));

				// Draw each message in turn.
				foreach (var message in Messages)
				{
					//Compute the alpha of this message.
					byte alpha = 255;

					if (message.State == ToastMessageState.FadingIn)
					{
						// Fading in.
						alpha = (byte)(255 * message.StateTimer.CurrentTime / FadeInTime);
					}
					else if (message.State == ToastMessageState.FadingOut)
					{
						// Fading out.
						alpha = (byte)(255 - (255 * message.StateTimer.CurrentTime / FadeOutTime));
					}
					else if (message.State == ToastMessageState.Dead)
					{
						continue;
					}

					//set teh text color
					var foregroundColor = message.Color;
					foregroundColor.A = alpha;

					// Draw the message text, with a drop shadow.
					var shadowColor = Color.Black;
					shadowColor.A = alpha;
					FontHelper.ShadowColor = shadowColor;
					FontHelper.Write(message.TextMessage,
									 currentMessagePosition,
									 Justify,
									 message.Scale,
									 foregroundColor,
									 spriteBatch,
									 Time);

					//Compute the message position.
					currentMessagePosition.Y = startPos.Y + (message.Position * (FontHelper.MeasureString(message.TextMessage).Y * message.Scale));
				}

				spriteBatch.End();
			}
		}

		/// <summary>
		/// Shows a new message.
		/// </summary>
		/// <param name="message">the text message to show</param>
		/// <param name="color">the color to write this message</param>
		public ToastMessage ShowMessage(string message, Color color, float scale = 1f)
		{
			lock (_lock)
			{
				float startPosition = Messages.Count;
				var toast = new ToastMessage(message, startPosition, color, scale);
				Messages.Add(toast);
				return toast;
			}
		}

		#endregion //Methods
	}
}
using FontBuddyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using GameTimer;

namespace ToastBuddyLib
{
	/// <summary>
	/// Component implements the IToastBuddy interface. 
	/// This is used to show notification messages when interesting events occur, 
	/// for instance when gamers join or leave the network session
	/// </summary>
	public class ToastBuddy : DrawableGameComponent, IToastBuddy
	{
		#region Fields

		/// <summary>
		/// deafult amount of time to fade in messages
		/// </summary>
		private readonly double _defaultFadeInTime = 0.25;

		/// <summary>
		/// deafult amount of time to show messages
		/// </summary>
		private readonly double _defaultShowTime = 5.0;

		/// <summary>
		/// default amount of time to fade out messages
		/// </summary>
		private readonly double _defaultFadeOutTime = 0.5;
		
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
		private readonly List<ToastMessage> messages = new List<ToastMessage>();

		/// <summary>
		/// Coordinates threadsafe access to the message list.
		/// </summary>
		private readonly object _lock = new object();

		#endregion //Fields

		#region Properties

		// Tweakable settings control how long each message is visible.
		public TimeSpan FadeInTime { get; set; }
		public TimeSpan ShowTime { get; set; }
		public TimeSpan FadeOutTime { get; set; }

		/// <summary>
		/// The font helper used to write the text with a little shadow
		/// </summary>
		private ShadowTextBuddy FontHelper { get; set; }

		/// <summary>
		/// The sprite batch used to write messages
		/// </summary>
		private SpriteBatch spriteBatch { get; set; }

		private Justify Justify { get; set; }

		private GameClock Time { get; set; }

		#endregion //Fields

		#region Initialization

		/// <summary>
		/// Constructs a new message display component.
		/// </summary>
		public ToastBuddy(Game game, 
			string fontResource,
			PositionDelegate messagePosition, 
			MatrixDelegate getMatrixDelegate, 
			Justify justify = Justify.Right) : base(game)
		{
			//grab those other items
			FontName = fontResource;
			DisplayPosition = messagePosition;
			GetMatrix = getMatrixDelegate;
			Justify = justify;

			FadeInTime = TimeSpan.FromSeconds(_defaultFadeInTime);
			ShowTime = TimeSpan.FromSeconds(_defaultShowTime);
			FadeOutTime = TimeSpan.FromSeconds(_defaultFadeOutTime);

			// Register ourselves to implement the IToastBuddy service.
			game.Components.Add(this);
			game.Services.AddService(typeof(IToastBuddy), this);

			//draw the toastbuddy on top of everything else
			DrawOrder = 100;

			Time = new GameClock();
			Time.Start();
		}

		/// <summary>
		/// Load graphics content for the message display.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			FontHelper = new ShadowTextBuddy
			{
				ShadowOffset = new Vector2(0.0f, 3.0f),
				ShadowSize = 1.0f,
				Font = Game.Content.Load<SpriteFont>(FontName)
			};
		}

		#endregion //Initialization

		#region Update and Draw

		/// <summary>
		/// Updates the message display component.
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			lock (_lock)
			{
				Time.Update(gameTime);

				//Every message wants to be the first one in line.
				float targetPosition = 0;

				// Update each message in turn.
				int index = 0;
				while (index < messages.Count)
				{
					var message = messages[index];

					// Gradually slide the message toward its desired position.
					var positionDelta = targetPosition - message.Position;
					var velocity = (float)gameTime.ElapsedGameTime.TotalSeconds * 2;
					message.Position += positionDelta * Math.Min(velocity, 1);

					// Update the age of the message.
					message.Age += gameTime.ElapsedGameTime;

					if (message.Age < ShowTime + FadeOutTime)
					{
						// This message is still alive.
						index++;

						// Any subsequent messages should be positioned below this one, unless it has started to fade out.
						if (message.Age < ShowTime)
						{
							targetPosition++;
						}
					}
					else
					{
						// This message is old, and should be removed.
						messages.RemoveAt(index);
					}
				}
			}
		}

		/// <summary>
		/// Draws the message display component.
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			lock (_lock)
			{
				// Early out if there are no messages to display.
				if (messages.Count == 0)
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
				foreach (var message in messages)
				{
					//Compute the alpha of this message.
					byte alpha = 255;
					if (message.Age < FadeInTime)
					{
						// Fading in.
						alpha = (byte)(255 * message.Age.TotalSeconds / FadeInTime.TotalSeconds);
					}
					else if (message.Age > ShowTime)
					{
						// Fading out.
						TimeSpan fadeOut = ShowTime + FadeOutTime - message.Age;
						alpha = (byte)(255 * fadeOut.TotalSeconds / FadeOutTime.TotalSeconds);
					}

					//Set the shadow color
					FontHelper.ShadowColor = new Color(Color.Black, alpha);

					//set teh text color
					var foregroundColor = message.Color;
					foregroundColor.A = alpha;

					//Compute the message position.
					currentMessagePosition.Y = startPos.Y + (message.Position * FontHelper.Font.LineSpacing);

					// Draw the message text, with a drop shadow.
					FontHelper.Write(message.TextMessage,
					                 currentMessagePosition,
									 Justify,
					                 1.0f,
									 foregroundColor,
					                 spriteBatch,
					                 Time);
				}

				spriteBatch.End();
			}
		}

		#endregion //Update and Draw

		#region Implement IToastBuddy

		/// <summary>
		/// Shows a new message.
		/// </summary>
		/// <param name="message">the text message to show</param>
		/// <param name="color">the color to write this message</param>
		public void ShowMessage(string message, Color color)
		{
			lock (_lock)
			{
				float startPosition = messages.Count;
				messages.Add(new ToastMessage(message, startPosition, color));
			}
		}

		/// <summary>
		/// Shows a new notification message with formatted text.
		/// </summary>
		public void ShowFormattedMessage(string message, Color color, params object[] parameters)
		{
			var formattedMessage = string.Format(message, parameters);

			lock (_lock)
			{
				var startPosition = messages.Count;
				messages.Add(new ToastMessage(formattedMessage, startPosition, color));
			}
		}

		#endregion //Implement IToastBuddy
	}
}
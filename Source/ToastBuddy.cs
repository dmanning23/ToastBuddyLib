using System;
using System.Collections.Generic;
using FontBuddyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ToastBuddyLib
{
	/// <summary>
	/// Component implements the IMessageDisplay interface. 
	/// This is used to show notification messages when interesting events occur, 
	/// for instance when gamers join or leave the network session
	/// </summary>
	public class ToastBuddy : DrawableGameComponent, IMessageDisplay
	{
		#region Fields

		private static readonly TimeSpan fadeInTime = TimeSpan.FromSeconds(0.25);

		private static readonly TimeSpan showTime = TimeSpan.FromSeconds(5);

		private static readonly TimeSpan fadeOutTime = TimeSpan.FromSeconds(0.5);

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
		private readonly List<NotificationMessage> messages = new List<NotificationMessage>();

		/// <summary>
		/// Coordinates threadsafe access to the message list.
		/// </summary>
		private readonly object syncObject = new object();

		/// <summary>
		/// The font helper used to write the text with a little shadow
		/// </summary>
		private ShadowTextBuddy FontHelper;

		/// <summary>
		/// The sprite batch used to write messages
		/// </summary>
		private SpriteBatch spriteBatch;

		// Tweakable settings control how long each message is visible.

		#endregion //Fields

		#region Initialization

		/// <summary>
		/// Constructs a new message display component.
		/// </summary>
		public ToastBuddy(Game game, string FontResource, PositionDelegate messagePosition, MatrixDelegate getMatrixDelegate) : base(game)
		{
			//grab those other items
			FontName = FontResource;
			DisplayPosition = messagePosition;
			GetMatrix = getMatrixDelegate;

			// Register ourselves to implement the IMessageDisplay service.
			game.Services.AddService(typeof (IMessageDisplay), this);
		}

		/// <summary>
		/// Load graphics content for the message display.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			FontHelper = new ShadowTextBuddy();
			FontHelper.ShadowOffset = new Vector2(0.0f, 3.0f);
			FontHelper.ShadowSize = 1.0f;
			FontHelper.Font = Game.Content.Load<SpriteFont>(FontName);
		}

		#endregion //Initialization

		#region Update and Draw

		/// <summary>
		/// Updates the message display component.
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			lock (syncObject)
			{
				//Every message wants to be the first one in line.
				float targetPosition = 0;

				// Update each message in turn.
				int index = 0;
				while (index < messages.Count)
				{
					NotificationMessage message = messages[index];

					// Gradually slide the message toward its desired position.
					float positionDelta = targetPosition - message.Position;
					float velocity = (float)gameTime.ElapsedGameTime.TotalSeconds * 2;
					message.Position += positionDelta * Math.Min(velocity, 1);

					// Update the age of the message.
					message.Age += gameTime.ElapsedGameTime;

					if (message.Age < showTime + fadeOutTime)
					{
						// This message is still alive.
						index++;

						// Any subsequent messages should be positioned below this one, unless it has started to fade out.
						if (message.Age < showTime)
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
			lock (syncObject)
			{
				// Early out if there are no messages to display.
				if (messages.Count == 0)
				{
					return;
				}

				//The start position that messages will be displayed at!
				Vector2 startPos = ((null != DisplayPosition) ? DisplayPosition() : Vector2.Zero);
				Vector2 currentMessagePosition = startPos;

				spriteBatch.Begin(SpriteSortMode.Deferred,
				                  BlendState.NonPremultiplied,
				                  null,
				                  null,
				                  null,
				                  null,
				                  ((null != GetMatrix) ? GetMatrix() : Matrix.Identity));

				// Draw each message in turn.
				foreach (NotificationMessage message in messages)
				{
					//Compute the alpha of this message.
					byte alpha = 255;
					if (message.Age < fadeInTime)
					{
						// Fading in.
						alpha = (byte)(255 * message.Age.TotalSeconds / fadeInTime.TotalSeconds);
					}
					else if (message.Age > showTime)
					{
						// Fading out.
						TimeSpan fadeOut = showTime + fadeOutTime - message.Age;
						alpha = (byte)(255 * fadeOut.TotalSeconds / fadeOutTime.TotalSeconds);
					}

					//Set the shadow color
					FontHelper.ShadowColor = new Color(0, 0, 0, alpha);

					//Compute the message position.
					currentMessagePosition.Y = startPos.Y + (message.Position * FontHelper.Font.LineSpacing);

					// Draw the message text, with a drop shadow.
					FontHelper.Write(message.TextMessage,
					                 currentMessagePosition,
					                 Justify.Right,
					                 1.0f,
					                 new Color(255, 255, 0, alpha),
					                 spriteBatch,
					                 0);
				}

				spriteBatch.End();
			}
		}

		#endregion //Update and Draw

		#region Implement IMessageDisplay

		/// <summary>
		/// Shows a new message.
		/// </summary>
		/// <param name="message">the text message to show</param>
		public void ShowMessage(string message)
		{
			lock (syncObject)
			{
				float startPosition = messages.Count;
				messages.Add(new NotificationMessage(message, startPosition));
			}
		}

		/// <summary>
		/// Shows a new notification message with formatted text.
		/// </summary>
		public void ShowFormattedMessage(string message, params object[] parameters)
		{
			string formattedMessage = string.Format(message, parameters);

			lock (syncObject)
			{
				float startPosition = messages.Count;
				messages.Add(new NotificationMessage(formattedMessage, startPosition));
			}
		}

		#endregion
	}
}
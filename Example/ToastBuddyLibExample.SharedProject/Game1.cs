using FontBuddyLib;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ResolutionBuddy;
using System;
using ToastBuddyLib;

namespace ToastBuddyLibExample
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		ToastBuddy messages;

		private InputState input = new InputState();
		private ControllerWrapper controller;

		private GameClock clock;

		FontBuddy instructionFont;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
			Content.RootDirectory = "Content";

			var resolution = new ResolutionComponent(this, graphics, new Point(1280, 720), new Point(1280, 720), false, false, false);

			messages = new ToastBuddy(this, "Fonts\\ArialBlack48", UpperRight, Resolution.TransformationMatrix, Justify.Right);
			//var rollingToast = messages.Toast as RollingToast;
			//rollingToast.ShowTime = 1.0f;
			messages.Toast = new ClearToast("Fonts\\ArialBlack48", UpperRight, Resolution.TransformationMatrix, Justify.Right);

			controller = new ControllerWrapper(0);
			Mappings.UseKeyboard[0] = true;
			clock = new GameClock();
		}

		public Vector2 UpperRight()
		{
			return new Vector2(Resolution.TitleSafeArea.Right, Resolution.TitleSafeArea.Top);
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			instructionFont = new FontBuddy();
			instructionFont.LoadContent(Content, @"Fonts\ArialBlack24");
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
				(Keyboard.GetState().IsKeyDown(Keys.Escape)))
			{
#if !__IOS__
				Exit();
#endif
			}

			//Update the input
			clock.Update(gameTime);
			input.Update();
			controller.Update(input);

			//Get the toast message component
			IServiceProvider services = Services;
			var messageDisplay = (IToastBuddy)services.GetService(typeof(IToastBuddy));

			//check for button presses
			for (EKeystroke i = 0; i < EKeystroke.Neutral; i++)
			{
				//if this button state changed, pop up a message
				if (controller.CheckKeystroke(i))
				{
					//pop up a message
					messageDisplay.ShowFormattedMessage("Pressed {0}", Color.Yellow, i.ToString());
				}
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Space))
			{
				//pop up a message
				messageDisplay.ShowMessage("Pressed Space!", Color.Yellow);

				var toast = messages.Toast as ClearToast;
				if (null != toast)
				{
					toast.ClearMessages();
				}
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

#if WINDOWS
			Resolution.ResetViewport();
#endif

			spriteBatch.Begin();

			//TODO: Add your drawing code here
			instructionFont.Write("Press any direction on the controller to pop up messages",
								  new Vector2(Resolution.TitleSafeArea.Left, Resolution.TitleSafeArea.Top),
								  Justify.Left,
								  0.75f,
								  Color.White,
								  spriteBatch,
								  clock);

			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}

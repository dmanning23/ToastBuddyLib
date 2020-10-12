using FontBuddyLib;
using Microsoft.Xna.Framework;

namespace ToastBuddyLib
{
	/// <summary>
	/// Component implements the IToastBuddy interface. 
	/// This is used to show notification messages when interesting events occur, 
	/// for instance when gamers join or leave the network session
	/// </summary>
	public class ToastBuddy : DrawableGameComponent, IToastBuddy, IDrawable, IUpdateable
	{
		#region Properties

		public BaseToastBuddy Toast { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Constructs a new message display component.
		/// </summary>
		public ToastBuddy(Game game,
			string fontResource,
			PositionDelegate messagePosition,
			MatrixDelegate getMatrixDelegate,
			Justify justify = Justify.Right,
			bool useFontPlus = false,
			int fontSize = 48) : base(game)
		{
			Toast = new RollingToast(fontResource, messagePosition, getMatrixDelegate, justify, useFontPlus, fontSize);

			// Register ourselves to implement the IToastBuddy service.
			game.Components.Add(this);
			game.Services.AddService(typeof(IToastBuddy), this);

			//draw the toastbuddy on top of everything else
			DrawOrder = 100;
		}

		/// <summary>
		/// Load graphics content for the message display.
		/// </summary>
		protected override void LoadContent()
		{
			Toast.LoadContent(GraphicsDevice, Game.Content);
		}

		/// <summary>
		/// Updates the message display component.
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			Toast.Update(gameTime);
		}

		/// <summary>
		/// Draws the message display component.
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			Toast.Draw(gameTime);
		}

		/// <summary>
		/// Shows a new message.
		/// </summary>
		/// <param name="message">the text message to show</param>
		/// <param name="color">the color to write this message</param>
		public ToastMessage ShowMessage(string message, Color color)
		{
			return Toast.ShowMessage(message, color);
		}

		/// <summary>
		/// Shows a new notification message with formatted text.
		/// </summary>
		public ToastMessage ShowFormattedMessage(string message, Color color, params object[] parameters)
		{
			return Toast.ShowFormattedMessage(message, color, parameters);
		}

		#endregion //Methods
	}
}
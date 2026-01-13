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

		private string FontResource { get; set; }
		private bool UseFontPlus { get; set; } = false;
		private int FontSize { get; set; } = 48;

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
			FontResource = fontResource;
			UseFontPlus = useFontPlus;
			FontSize = fontSize;

			Toast = new RollingToast(messagePosition, getMatrixDelegate, justify);

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
			Toast.LoadContent(GraphicsDevice, Game.Content, FontResource, UseFontPlus, FontSize);
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
			Toast.Draw();
		}

		/// <summary>
		/// Shows a new message.
		/// </summary>
		/// <param name="message">the text message to show</param>
		/// <param name="color">the color to write this message</param>
		public ToastMessage ShowMessage(string message, Color color, float scale = 1f)
		{
			return Toast.ShowMessage(message, color, scale);
		}

		#endregion //Methods
	}
}
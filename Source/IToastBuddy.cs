using Microsoft.Xna.Framework;

namespace ToastBuddyLib
{
	/// <summary>
	/// Interface used to display notification messages when interesting events occur,
	/// for instance when gamers join or leave the network session. This interface
	/// is registered as a service, so any piece of code wanting to display a message
	/// can look it up from Game.Services, without needing to worry about how the
	/// message display is implemented. In this sample, the MessageDisplayComponent
	/// class implement this IToastBuddy service.
	/// </summary>
	public interface IToastBuddy : IDrawable, IUpdateable
	{
		void ShowMessage(string message, Color color);

		void ShowFormattedMessage(string message, Color color, params object[] parameters);
	}
}
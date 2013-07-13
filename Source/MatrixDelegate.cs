using Microsoft.Xna.Framework;

namespace ToastBuddyLib
{
	/// <summary>
	/// This is a callback method for getting a matrix
	/// used to break out dependencies
	/// </summary>
	/// <returns>a method to get a matrix.</returns>
	public delegate Matrix MatrixDelegate();
}

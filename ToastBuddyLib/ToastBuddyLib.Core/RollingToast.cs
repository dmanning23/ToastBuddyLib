using FontBuddyLib;

namespace ToastBuddyLib
{
	/// <summary>
	/// Component implements the IToastBuddy interface. 
	/// This is used to show notification messages when interesting events occur, 
	/// for instance when gamers join or leave the network session
	/// </summary>
	public class RollingToast : BaseToastBuddy
	{
		#region Properties

		/// <summary>
		/// deafult amount of time to show messages
		/// </summary>
		private readonly float _defaultShowTime = 5.0f;

		public float ShowTime { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Constructs a new message display component.
		/// </summary>
		public RollingToast(PositionDelegate messagePosition,
			MatrixDelegate getMatrixDelegate,
			Justify justify = Justify.Right) : base(messagePosition, getMatrixDelegate, justify)
		{
			ShowTime = _defaultShowTime;
		}

		protected override void UpdateMessage(ToastMessage message)
		{
			message.Update(Time);

			switch (message.State)
			{
				case ToastMessageState.FadingIn:
					{
						if (message.StateTimer.CurrentTime >= FadeInTime)
						{
							message.StateTimer.Start();
							message.State = ToastMessageState.Showing;
						}
					}
					break;
				case ToastMessageState.Showing:
					{
						if (message.StateTimer.CurrentTime >= ShowTime)
						{
							message.StateTimer.Start();
							message.State = ToastMessageState.FadingOut;
						}
					}
					break;
				case ToastMessageState.FadingOut:
					{
						if (message.StateTimer.CurrentTime >= FadeOutTime)
						{
							message.State = ToastMessageState.Dead;
						}
					}
					break;
			}
	}

		#endregion //Methods
	}
}
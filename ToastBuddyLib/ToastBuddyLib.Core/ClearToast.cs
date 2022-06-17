using FontBuddyLib;

namespace ToastBuddyLib
{
	/// <summary>
	/// This is a toast strategy where the messages are displayed until they are cleared out.
	/// </summary>
	public class ClearToast : BaseToastBuddy
	{
		#region Methods

		/// <summary>
		/// Constructs a new message display component.
		/// </summary>
		public ClearToast(PositionDelegate messagePosition,
			MatrixDelegate getMatrixDelegate,
			Justify justify = Justify.Right) : base(messagePosition, getMatrixDelegate, justify)
		{
		}

		public void ClearMessages()
		{
			foreach (var message in Messages)
			{
				message.StateTimer.Start();
				message.State = ToastMessageState.FadingOut;
			}
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

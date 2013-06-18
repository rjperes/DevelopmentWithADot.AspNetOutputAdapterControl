using System;

namespace DevelopmentWithADot.AspNetOutputAdapterControl
{
	[Serializable]
	public class OutputEventArgs : EventArgs
	{
		public String Html
		{
			get;
			set;
		}
	}
}
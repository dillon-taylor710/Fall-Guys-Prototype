using Newtonsoft.Json.Shims;
using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public class ErrorEventArgs : EventArgs
	{
		private object CurrentObject
		{
			[CompilerGenerated]
			set
			{
				CurrentObject = value;
			}
		}

		private ErrorContext ErrorContext
		{
			[CompilerGenerated]
			set
			{
				ErrorContext = value;
			}
		}

		public ErrorEventArgs(object currentObject, ErrorContext errorContext)
		{
			CurrentObject = currentObject;
			ErrorContext = errorContext;
		}
	}
}

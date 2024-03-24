using Newtonsoft.Json.Shims;
using System;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public class ErrorContext
	{
		internal bool Traced
		{
			get;
			set;
		}

		public Exception Error
		{
			get;
			private set;
		}

		private object OriginalObject
		{
			[CompilerGenerated]
			set
			{
				OriginalObject = value;
			}
		}

		private object Member
		{
			[CompilerGenerated]
			set
			{
				Member = value;
			}
		}

		private string Path
		{
			[CompilerGenerated]
			set
			{
				Path = value;
			}
		}

		public bool Handled
		{
			get;
		}

		internal ErrorContext(object originalObject, object member, string path, Exception error)
		{
			OriginalObject = originalObject;
			Member = member;
			Error = error;
			Path = path;
		}
	}
}

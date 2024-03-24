using Newtonsoft.Json.Shims;
using System.Runtime.CompilerServices;

namespace System.ComponentModel
{
	[Preserve]
	public class PropertyChangingEventArgs : EventArgs
	{
		public virtual string PropertyName
		{
			[CompilerGenerated]
			set
			{
				PropertyName = value;
			}
		}

		public PropertyChangingEventArgs(string propertyName)
		{
			PropertyName = propertyName;
		}
	}
}

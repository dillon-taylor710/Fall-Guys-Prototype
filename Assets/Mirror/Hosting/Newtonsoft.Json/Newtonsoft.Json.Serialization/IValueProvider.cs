using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Serialization
{
	[Preserve]
	public interface IValueProvider
	{
		void SetValue(object target, object value);

		object GetValue(object target);
	}
}

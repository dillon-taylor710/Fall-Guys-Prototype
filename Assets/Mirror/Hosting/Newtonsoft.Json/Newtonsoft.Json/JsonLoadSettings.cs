using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Linq
{
	[Preserve]
	public class JsonLoadSettings
	{
		private CommentHandling _commentHandling;

		private LineInfoHandling _lineInfoHandling;

		public CommentHandling CommentHandling
		{
			get
			{
				return _commentHandling;
			}
		}

		public LineInfoHandling LineInfoHandling
		{
			get
			{
				return _lineInfoHandling;
			}
		}
	}
}

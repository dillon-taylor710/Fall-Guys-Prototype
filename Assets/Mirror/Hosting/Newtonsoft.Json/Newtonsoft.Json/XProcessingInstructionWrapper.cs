using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	internal class XProcessingInstructionWrapper : XObjectWrapper
	{
		private XProcessingInstruction ProcessingInstruction
		{
			get
			{
				return (XProcessingInstruction)base.WrappedNode;
			}
		}

		public override string LocalName
		{
			get
			{
				return ProcessingInstruction.Target;
			}
		}

		public override string Value
		{
			get
			{
				return ProcessingInstruction.Data;
			}
			set
			{
				ProcessingInstruction.Data = value;
			}
		}

		public XProcessingInstructionWrapper(XProcessingInstruction processingInstruction)
			: base(processingInstruction)
		{
		}
	}
}

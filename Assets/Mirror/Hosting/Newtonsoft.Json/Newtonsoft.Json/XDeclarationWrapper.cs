using System.Xml;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	internal class XDeclarationWrapper : XObjectWrapper, IXmlDeclaration, IXmlNode
	{
		internal XDeclaration Declaration
		{
			get;
			private set;
		}

		public override XmlNodeType NodeType
		{
			get
			{
				return XmlNodeType.XmlDeclaration;
			}
		}

		public string Version
		{
			get
			{
				return Declaration.Version;
			}
		}

		public string Encoding
		{
			get
			{
				return Declaration.Encoding;
			}
		}

		public string Standalone
		{
			get
			{
				return Declaration.Standalone;
			}
		}

		public XDeclarationWrapper(XDeclaration declaration)
			: base(null)
		{
			Declaration = declaration;
		}
	}
}

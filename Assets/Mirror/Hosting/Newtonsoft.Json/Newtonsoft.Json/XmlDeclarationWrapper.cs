using System.Xml;

namespace Newtonsoft.Json.Converters
{
	internal class XmlDeclarationWrapper : XmlNodeWrapper, IXmlDeclaration, IXmlNode
	{
		private readonly XmlDeclaration _declaration;

		public string Version
		{
			get
			{
				return _declaration.Version;
			}
		}

		public string Encoding
		{
			get
			{
				return _declaration.Encoding;
			}
		}

		public string Standalone
		{
			get
			{
				return _declaration.Standalone;
			}
		}

		public XmlDeclarationWrapper(XmlDeclaration declaration)
			: base(declaration)
		{
			_declaration = declaration;
		}
	}
}

using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	internal class XDocumentTypeWrapper : XObjectWrapper, IXmlDocumentType, IXmlNode
	{
		private readonly XDocumentType _documentType;

		public string Name
		{
			get
			{
				return _documentType.Name;
			}
		}

		public string System
		{
			get
			{
				return _documentType.SystemId;
			}
		}

		public string Public
		{
			get
			{
				return _documentType.PublicId;
			}
		}

		public string InternalSubset
		{
			get
			{
				return _documentType.InternalSubset;
			}
		}

		public override string LocalName
		{
			get
			{
				return "DOCTYPE";
			}
		}

		public XDocumentTypeWrapper(XDocumentType documentType)
			: base(documentType)
		{
			_documentType = documentType;
		}
	}
}

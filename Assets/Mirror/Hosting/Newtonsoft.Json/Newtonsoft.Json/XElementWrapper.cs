using System.Collections.Generic;
using System.Xml.Linq;

namespace Newtonsoft.Json.Converters
{
	internal class XElementWrapper : XContainerWrapper, IXmlElement, IXmlNode
	{
		private List<IXmlNode> _attributes;

		private XElement Element
		{
			get
			{
				return (XElement)base.WrappedNode;
			}
		}

		public override List<IXmlNode> Attributes
		{
			get
			{
				if (_attributes == null)
				{
					_attributes = new List<IXmlNode>();
					foreach (XAttribute item in Element.Attributes())
					{
						_attributes.Add(new XAttributeWrapper(item));
					}
					string namespaceUri = NamespaceUri;
					if (!string.IsNullOrEmpty(namespaceUri))
					{
						string a = namespaceUri;
						IXmlNode parentNode = ParentNode;
						if (a != ((parentNode != null) ? parentNode.NamespaceUri : null) && string.IsNullOrEmpty(GetPrefixOfNamespace(namespaceUri)))
						{
							bool flag = false;
							foreach (IXmlNode attribute in _attributes)
							{
								if (attribute.LocalName == "xmlns" && string.IsNullOrEmpty(attribute.NamespaceUri) && attribute.Value == namespaceUri)
								{
									flag = true;
								}
							}
							if (!flag)
							{
								_attributes.Insert(0, new XAttributeWrapper(new XAttribute("xmlns", namespaceUri)));
							}
						}
					}
				}
				return _attributes;
			}
		}

		public override string Value
		{
			get
			{
				return Element.Value;
			}
			set
			{
				Element.Value = value;
			}
		}

		public override string LocalName
		{
			get
			{
				return Element.Name.LocalName;
			}
		}

		public override string NamespaceUri
		{
			get
			{
				return Element.Name.NamespaceName;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return Element.IsEmpty;
			}
		}

		public XElementWrapper(XElement element)
			: base(element)
		{
		}

		public void SetAttributeNode(IXmlNode attribute)
		{
			XObjectWrapper xObjectWrapper = (XObjectWrapper)attribute;
			Element.Add(xObjectWrapper.WrappedNode);
			_attributes = null;
		}

		public override IXmlNode AppendChild(IXmlNode newChild)
		{
			IXmlNode result = base.AppendChild(newChild);
			_attributes = null;
			return result;
		}

		public string GetPrefixOfNamespace(string namespaceUri)
		{
			return Element.GetPrefixOfNamespace(namespaceUri);
		}
	}
}

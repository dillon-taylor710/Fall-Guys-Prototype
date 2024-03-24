using System.Collections.Generic;
using System.Xml;

namespace Newtonsoft.Json.Converters
{
	internal class XmlNodeWrapper : IXmlNode
	{
		private readonly XmlNode _node;

		private List<IXmlNode> _childNodes;

		private List<IXmlNode> _attributes;

		public object WrappedNode
		{
			get
			{
				return _node;
			}
		}

		public XmlNodeType NodeType
		{
			get
			{
				return _node.NodeType;
			}
		}

		public virtual string LocalName
		{
			get
			{
				return _node.LocalName;
			}
		}

		public List<IXmlNode> ChildNodes
		{
			get
			{
				if (_childNodes == null)
				{
					_childNodes = new List<IXmlNode>(_node.ChildNodes.Count);
					foreach (XmlNode childNode in _node.ChildNodes)
					{
						_childNodes.Add(WrapNode(childNode));
					}
				}
				return _childNodes;
			}
		}

		public List<IXmlNode> Attributes
		{
			get
			{
				if (_node.Attributes == null)
				{
					return null;
				}
				if (_attributes == null)
				{
					_attributes = new List<IXmlNode>(_node.Attributes.Count);
					foreach (XmlAttribute attribute in _node.Attributes)
					{
						_attributes.Add(WrapNode(attribute));
					}
				}
				return _attributes;
			}
		}

		public IXmlNode ParentNode
		{
			get
			{
				XmlNode xmlNode = (_node is XmlAttribute) ? ((XmlAttribute)_node).OwnerElement : _node.ParentNode;
				if (xmlNode == null)
				{
					return null;
				}
				return WrapNode(xmlNode);
			}
		}

		public string Value
		{
			get
			{
				return _node.Value;
			}
			set
			{
				_node.Value = value;
			}
		}

		public string NamespaceUri
		{
			get
			{
				return _node.NamespaceURI;
			}
		}

		public XmlNodeWrapper(XmlNode node)
		{
			_node = node;
		}

		internal static IXmlNode WrapNode(XmlNode node)
		{
			switch (node.NodeType)
			{
			case XmlNodeType.Element:
				return new XmlElementWrapper((XmlElement)node);
			case XmlNodeType.XmlDeclaration:
				return new XmlDeclarationWrapper((XmlDeclaration)node);
			case XmlNodeType.DocumentType:
				return new XmlDocumentTypeWrapper((XmlDocumentType)node);
			default:
				return new XmlNodeWrapper(node);
			}
		}

		public IXmlNode AppendChild(IXmlNode newChild)
		{
			XmlNodeWrapper xmlNodeWrapper = (XmlNodeWrapper)newChild;
			_node.AppendChild(xmlNodeWrapper._node);
			_childNodes = null;
			_attributes = null;
			return newChild;
		}
	}
}

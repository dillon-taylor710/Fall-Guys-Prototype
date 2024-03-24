using Newtonsoft.Json.Shims;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace Newtonsoft.Json.Linq
{
	[Preserve]
	public abstract class JContainer : JToken, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, ICollection, IEnumerable, IList
	{
		internal ListChangedEventHandler _listChanged;

		internal AddingNewEventHandler _addingNew;

		private object _syncRoot;

		private bool _busy;

		protected abstract IList<JToken> ChildrenTokens
		{
			get;
		}

		public override bool HasValues
		{
			get
			{
				return ChildrenTokens.Count > 0;
			}
		}

		public override JToken First
		{
			get
			{
				IList<JToken> childrenTokens = ChildrenTokens;
				if (childrenTokens.Count <= 0)
				{
					return null;
				}
				return childrenTokens[0];
			}
		}

		public override JToken Last
		{
			get
			{
				IList<JToken> childrenTokens = ChildrenTokens;
				int count = childrenTokens.Count;
				if (count <= 0)
				{
					return null;
				}
				return childrenTokens[count - 1];
			}
		}

		JToken IList<JToken>.this[int index]
		{
			get
			{
				return GetItem(index);
			}
			set
			{
				SetItem(index, value);
			}
		}

		bool ICollection<JToken>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		bool IList.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		object IList.this[int index]
		{
			get
			{
				return GetItem(index);
			}
			set
			{
				SetItem(index, EnsureValue(value));
			}
		}

		public int Count
		{
			get
			{
				return ChildrenTokens.Count;
			}
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				}
				return _syncRoot;
			}
		}

		internal JContainer()
		{
		}

		internal JContainer(JContainer other)
			: this()
		{
			ValidationUtils.ArgumentNotNull(other, "other");
			int num = 0;
			foreach (JToken item in (IEnumerable<JToken>)other)
			{
				AddInternal(num, item, false);
				num++;
			}
		}

		internal void CheckReentrancy()
		{
			if (_busy)
			{
				throw new InvalidOperationException("Cannot change {0} during a collection change event.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
		}

		protected virtual void OnListChanged(ListChangedEventArgs e)
		{
			ListChangedEventHandler listChanged = _listChanged;
			if (listChanged != null)
			{
				_busy = true;
				try
				{
					listChanged(this, e);
				}
				finally
				{
					_busy = false;
				}
			}
		}

		internal bool ContentsEqual(JContainer container)
		{
			if (container == this)
			{
				return true;
			}
			IList<JToken> childrenTokens = ChildrenTokens;
			IList<JToken> childrenTokens2 = container.ChildrenTokens;
			if (childrenTokens.Count != childrenTokens2.Count)
			{
				return false;
			}
			for (int i = 0; i < childrenTokens.Count; i++)
			{
				if (!childrenTokens[i].DeepEquals(childrenTokens2[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override JEnumerable<JToken> Children()
		{
			return new JEnumerable<JToken>(ChildrenTokens);
		}

		internal bool IsMultiContent(object content)
		{
			if (content is IEnumerable && !(content is string) && !(content is JToken))
			{
				return !(content is byte[]);
			}
			return false;
		}

		internal JToken EnsureParentToken(JToken item, bool skipParentCheck)
		{
			if (item == null)
			{
				return JValue.CreateNull();
			}
			if (skipParentCheck)
			{
				return item;
			}
			if (item.Parent != null || item == this || (item.HasValues && base.Root == item))
			{
				item = item.CloneToken();
			}
			return item;
		}

		internal abstract int IndexOfItem(JToken item);

		internal virtual void InsertItem(int index, JToken item, bool skipParentCheck)
		{
			IList<JToken> childrenTokens = ChildrenTokens;
			if (index > childrenTokens.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Index must be within the bounds of the List.");
			}
			CheckReentrancy();
			item = EnsureParentToken(item, skipParentCheck);
			JToken jToken = (index == 0) ? null : childrenTokens[index - 1];
			JToken jToken2 = (index == childrenTokens.Count) ? null : childrenTokens[index];
			ValidateToken(item, null);
			item.Parent = this;
			item.Previous = jToken;
			if (jToken != null)
			{
				jToken.Next = item;
			}
			item.Next = jToken2;
			if (jToken2 != null)
			{
				jToken2.Previous = item;
			}
			childrenTokens.Insert(index, item);
			if (_listChanged != null)
			{
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
			}
		}

		internal virtual void RemoveItemAt(int index)
		{
			IList<JToken> childrenTokens = ChildrenTokens;
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", "Index is less than 0.");
			}
			if (index >= childrenTokens.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Index is equal to or greater than Count.");
			}
			CheckReentrancy();
			JToken jToken = childrenTokens[index];
			JToken jToken2 = (index == 0) ? null : childrenTokens[index - 1];
			JToken jToken3 = (index == childrenTokens.Count - 1) ? null : childrenTokens[index + 1];
			if (jToken2 != null)
			{
				jToken2.Next = jToken3;
			}
			if (jToken3 != null)
			{
				jToken3.Previous = jToken2;
			}
			jToken.Parent = null;
			jToken.Previous = null;
			jToken.Next = null;
			childrenTokens.RemoveAt(index);
			if (_listChanged != null)
			{
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
			}
		}

		internal virtual bool RemoveItem(JToken item)
		{
			int num = IndexOfItem(item);
			if (num >= 0)
			{
				RemoveItemAt(num);
				return true;
			}
			return false;
		}

		internal virtual JToken GetItem(int index)
		{
			return ChildrenTokens[index];
		}

		internal virtual void SetItem(int index, JToken item)
		{
			IList<JToken> childrenTokens = ChildrenTokens;
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", "Index is less than 0.");
			}
			if (index >= childrenTokens.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Index is equal to or greater than Count.");
			}
			JToken jToken = childrenTokens[index];
			if (!IsTokenUnchanged(jToken, item))
			{
				CheckReentrancy();
				item = EnsureParentToken(item, false);
				ValidateToken(item, jToken);
				JToken jToken2 = (index == 0) ? null : childrenTokens[index - 1];
				JToken jToken3 = (index == childrenTokens.Count - 1) ? null : childrenTokens[index + 1];
				item.Parent = this;
				item.Previous = jToken2;
				if (jToken2 != null)
				{
					jToken2.Next = item;
				}
				item.Next = jToken3;
				if (jToken3 != null)
				{
					jToken3.Previous = item;
				}
				childrenTokens[index] = item;
				jToken.Parent = null;
				jToken.Previous = null;
				jToken.Next = null;
				if (_listChanged != null)
				{
					OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
				}
			}
		}

		internal virtual void ClearItems()
		{
			CheckReentrancy();
			IList<JToken> childrenTokens = ChildrenTokens;
			foreach (JToken item in childrenTokens)
			{
				item.Parent = null;
				item.Previous = null;
				item.Next = null;
			}
			childrenTokens.Clear();
			if (_listChanged != null)
			{
				OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			}
		}

		internal virtual void ReplaceItem(JToken existing, JToken replacement)
		{
			if (existing != null && existing.Parent == this)
			{
				int index = IndexOfItem(existing);
				SetItem(index, replacement);
			}
		}

		internal virtual bool ContainsItem(JToken item)
		{
			return IndexOfItem(item) != -1;
		}

		internal virtual void CopyItemsTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
			}
			if (arrayIndex >= array.Length && arrayIndex != 0)
			{
				throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
			}
			if (Count > array.Length - arrayIndex)
			{
				throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
			}
			int num = 0;
			foreach (JToken childrenToken in ChildrenTokens)
			{
				array.SetValue(childrenToken, arrayIndex + num);
				num++;
			}
		}

		internal static bool IsTokenUnchanged(JToken currentValue, JToken newValue)
		{
			JValue jValue = currentValue as JValue;
			if (jValue != null)
			{
				if (jValue.Type == JTokenType.Null && newValue == null)
				{
					return true;
				}
				return jValue.Equals(newValue);
			}
			return false;
		}

		internal virtual void ValidateToken(JToken o, JToken existing)
		{
			ValidationUtils.ArgumentNotNull(o, "o");
			if (o.Type == JTokenType.Property)
			{
				throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
			}
		}

		public virtual void Add(object content)
		{
			AddInternal(ChildrenTokens.Count, content, false);
		}

		internal void AddAndSkipParentCheck(JToken token)
		{
			AddInternal(ChildrenTokens.Count, token, true);
		}

		internal void AddInternal(int index, object content, bool skipParentCheck)
		{
			if (IsMultiContent(content))
			{
				IEnumerable obj = (IEnumerable)content;
				int num = index;
				foreach (object item2 in obj)
				{
					AddInternal(num, item2, skipParentCheck);
					num++;
				}
			}
			else
			{
				JToken item = CreateFromContent(content);
				InsertItem(index, item, skipParentCheck);
			}
		}

		internal static JToken CreateFromContent(object content)
		{
			JToken jToken = content as JToken;
			if (jToken != null)
			{
				return jToken;
			}
			return new JValue(content);
		}

		public void RemoveAll()
		{
			ClearItems();
		}

		internal void ReadTokenFrom(JsonReader reader, JsonLoadSettings options)
		{
			int depth = reader.Depth;
			if (!reader.Read())
			{
				throw JsonReaderException.Create(reader, "Error reading {0} from JsonReader.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
			}
			ReadContentFrom(reader, options);
			if (reader.Depth > depth)
			{
				throw JsonReaderException.Create(reader, "Unexpected end of content while loading {0}.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
			}
		}

		internal void ReadContentFrom(JsonReader r, JsonLoadSettings settings)
		{
			ValidationUtils.ArgumentNotNull(r, "r");
			IJsonLineInfo lineInfo = r as IJsonLineInfo;
			JContainer jContainer = this;
			do
			{
				JProperty obj = jContainer as JProperty;
				if (((obj != null) ? obj.Value : null) != null)
				{
					if (jContainer == this)
					{
						break;
					}
					jContainer = jContainer.Parent;
				}
				switch (r.TokenType)
				{
				case JsonToken.StartArray:
				{
					JArray jArray = new JArray();
					jArray.SetLineInfo(lineInfo, settings);
					jContainer.Add(jArray);
					jContainer = jArray;
					break;
				}
				case JsonToken.EndArray:
					if (jContainer == this)
					{
						return;
					}
					jContainer = jContainer.Parent;
					break;
				case JsonToken.StartObject:
				{
					JObject jObject = new JObject();
					jObject.SetLineInfo(lineInfo, settings);
					jContainer.Add(jObject);
					jContainer = jObject;
					break;
				}
				case JsonToken.EndObject:
					if (jContainer == this)
					{
						return;
					}
					jContainer = jContainer.Parent;
					break;
				case JsonToken.StartConstructor:
				{
					JConstructor jConstructor = new JConstructor(r.Value.ToString());
					jConstructor.SetLineInfo(lineInfo, settings);
					jContainer.Add(jConstructor);
					jContainer = jConstructor;
					break;
				}
				case JsonToken.EndConstructor:
					if (jContainer == this)
					{
						return;
					}
					jContainer = jContainer.Parent;
					break;
				case JsonToken.Integer:
				case JsonToken.Float:
				case JsonToken.String:
				case JsonToken.Boolean:
				case JsonToken.Date:
				case JsonToken.Bytes:
				{
					JValue jValue = new JValue(r.Value);
					jValue.SetLineInfo(lineInfo, settings);
					jContainer.Add(jValue);
					break;
				}
				case JsonToken.Comment:
					if (settings != null && settings.CommentHandling == CommentHandling.Load)
					{
						JValue jValue = JValue.CreateComment(r.Value.ToString());
						jValue.SetLineInfo(lineInfo, settings);
						jContainer.Add(jValue);
					}
					break;
				case JsonToken.Null:
				{
					JValue jValue = JValue.CreateNull();
					jValue.SetLineInfo(lineInfo, settings);
					jContainer.Add(jValue);
					break;
				}
				case JsonToken.Undefined:
				{
					JValue jValue = JValue.CreateUndefined();
					jValue.SetLineInfo(lineInfo, settings);
					jContainer.Add(jValue);
					break;
				}
				case JsonToken.PropertyName:
				{
					string name = r.Value.ToString();
					JProperty jProperty = new JProperty(name);
					jProperty.SetLineInfo(lineInfo, settings);
					JProperty jProperty2 = ((JObject)jContainer).Property(name);
					if (jProperty2 == null)
					{
						jContainer.Add(jProperty);
					}
					else
					{
						jProperty2.Replace(jProperty);
					}
					jContainer = jProperty;
					break;
				}
				default:
					throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, r.TokenType));
				case JsonToken.None:
					break;
				}
			}
			while (r.Read());
		}

		internal int ContentsHashCode()
		{
			int num = 0;
			foreach (JToken childrenToken in ChildrenTokens)
			{
				num ^= childrenToken.GetDeepHashCode();
			}
			return num;
		}

		int IList<JToken>.IndexOf(JToken item)
		{
			return IndexOfItem(item);
		}

		void IList<JToken>.Insert(int index, JToken item)
		{
			InsertItem(index, item, false);
		}

		void IList<JToken>.RemoveAt(int index)
		{
			RemoveItemAt(index);
		}

		void ICollection<JToken>.Add(JToken item)
		{
			Add(item);
		}

		void ICollection<JToken>.Clear()
		{
			ClearItems();
		}

		bool ICollection<JToken>.Contains(JToken item)
		{
			return ContainsItem(item);
		}

		void ICollection<JToken>.CopyTo(JToken[] array, int arrayIndex)
		{
			CopyItemsTo(array, arrayIndex);
		}

		bool ICollection<JToken>.Remove(JToken item)
		{
			return RemoveItem(item);
		}

		private JToken EnsureValue(object value)
		{
			if (value == null)
			{
				return null;
			}
			JToken jToken = value as JToken;
			if (jToken != null)
			{
				return jToken;
			}
			throw new ArgumentException("Argument is not a JToken.");
		}

		int IList.Add(object value)
		{
			Add(EnsureValue(value));
			return Count - 1;
		}

		void IList.Clear()
		{
			ClearItems();
		}

		bool IList.Contains(object value)
		{
			return ContainsItem(EnsureValue(value));
		}

		int IList.IndexOf(object value)
		{
			return IndexOfItem(EnsureValue(value));
		}

		void IList.Insert(int index, object value)
		{
			InsertItem(index, EnsureValue(value), false);
		}

		void IList.Remove(object value)
		{
			RemoveItem(EnsureValue(value));
		}

		void IList.RemoveAt(int index)
		{
			RemoveItemAt(index);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			CopyItemsTo(array, index);
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetSerializer
{
	public sealed class StringSerializer : IStaticTypeSerializer
	{
		private static readonly Encoding encoding = new UTF8Encoding(false, true);

		public bool Handles(Type type)
		{
			return type == typeof(string);
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			return new Type[0];
		}

		public void Serialize(Serializer serializer, Type staticType, Stream stream, object ob)
		{
			string value = (string) ob;
			if (value == null)
			{
				Primitives.WritePrimitive(stream, (uint)0);
				return;
			}
			else if (value.Length == 0)
			{
				Primitives.WritePrimitive(stream, (uint)1);
				return;
			}

			int len = encoding.GetByteCount(value);

			Primitives.WritePrimitive(stream, (uint)len + 1);
			Primitives.WritePrimitive(stream, (uint)value.Length);

			var buf = new byte[len];

			encoding.GetBytes(value, 0, value.Length, buf, 0);

			stream.Write(buf, 0, len);
		}

		public object Deserialize(Serializer serializer, Type staticType, Stream stream)
		{
			uint len;
			Primitives.ReadPrimitive(stream, out len);

			if (len == 0)
			{
				return null;
			}
			else if (len == 1)
			{
				return string.Empty;;
			}

			uint totalChars;
			Primitives.ReadPrimitive(stream, out totalChars);

			len -= 1;

			var buf = new byte[len];

			int l = 0;

			while (l < len)
			{
				int r = stream.Read(buf, l, (int)len - l);
				if (r == 0)
					throw new EndOfStreamException();
				l += r;
			}

			return encoding.GetString(buf);
		}
	}
}
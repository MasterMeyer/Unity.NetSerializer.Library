using System;
using System.Collections.Generic;
using System.IO;

namespace NetSerializer
{
	public sealed class IntSerializer : IStaticTypeSerializer
	{
		public bool Handles(Type type)
		{
			return typeof(int) == type;
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			return new Type[0];
		}

		public void Serialize(Serializer serializer, Type staticType, Stream stream, object ob)
		{
			Primitives.WritePrimitive(stream, (int) ob);
		}

		public object Deserialize(Serializer serializer, Type staticType, Stream stream)
		{
			int result;
			Primitives.ReadPrimitive(stream, out result);
			return result;
		}
	}
}
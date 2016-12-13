using System;
using System.Collections.Generic;
using System.IO;

namespace NetSerializer
{
	public sealed class UIntSerializer : IStaticTypeSerializer
	{
		public bool Handles(Type type)
		{
			return typeof(uint) == type;
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			return new Type[0];
		}

		public void Serialize(Serializer serializer, Type staticType, Stream stream, object ob)
		{
			Primitives.WritePrimitive(stream, (uint) ob);
		}

		public object Deserialize(Serializer serializer, Type staticType, Stream stream)
		{
			uint result;
			Primitives.ReadPrimitive(stream, out result);
			return result;
		}
	}
}
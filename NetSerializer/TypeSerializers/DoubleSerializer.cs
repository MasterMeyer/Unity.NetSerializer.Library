using System;
using System.Collections.Generic;
using System.IO;

namespace NetSerializer
{
	public sealed class DoubleSerializer : IStaticTypeSerializer
	{
		public bool Handles(Type type)
		{
			return typeof(double) == type;
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			return new Type[0];
		}

		public void Serialize(Serializer serializer, Type staticType, Stream stream, object ob)
		{
			Primitives.WritePrimitive(stream, (double) ob);
		}

		public object Deserialize(Serializer serializer, Type staticType, Stream stream)
		{
			double result;
			Primitives.ReadPrimitive(stream, out result);
			return result;
		}
	}
}
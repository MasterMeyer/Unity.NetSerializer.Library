/*
 * Copyright 2015 Tomi Valkeinen
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NetSerializer
{
	sealed class PrimitivesSerializer : IStaticTypeSerializer
	{
		static Type[] s_primitives = new Type[] {
				typeof(byte), typeof(sbyte),
				typeof(char),
				typeof(ushort), typeof(short),
				typeof(ulong),
				typeof(float),
				typeof(DateTime),
				typeof(byte[]),
				typeof(Decimal),
			};

		public bool Handles(Type type)
		{
			return s_primitives.Contains(type);
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			return new Type[0];
		}

		public void Serialize(Serializer serializer, Type staticType, Stream stream, object ob)
		{
			MethodInfo method = Primitives.GetWritePrimitive(staticType);
			method.Invoke(null , new object[] {stream, ob});
		}

		public object Deserialize(Serializer serializer, Type staticType, Stream stream)
		{
			MethodInfo method = Primitives.GetReaderPrimitive(staticType);
			object[] parameters = new object[] {stream, null};
			method.Invoke(null , parameters);
			// return out parameter
			return parameters[1];
		}


		public static IEnumerable<Type> GetSupportedTypes()
		{
			return s_primitives;
		}
	}
}

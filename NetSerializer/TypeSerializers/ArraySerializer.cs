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

namespace NetSerializer
{
	sealed class ArraySerializer : IStaticTypeSerializer
	{
		public bool Handles(Type type)
		{
			if (!type.IsArray)
				return false;

			if (type.GetArrayRank() != 1)
				throw new NotSupportedException(String.Format("Multi-dim arrays not supported: {0}", type.FullName));

			return true;
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			return new[] { typeof(uint), type.GetElementType() };
		}

		public void Serialize(Serializer serializer, Type staticType, Stream stream, object ob)
		{
			if (ob == null)
			{
				Primitives.WritePrimitive(stream, (uint) 0);
			}
			else
			{
				Type elementType = ob.GetType().GetElementType();
				Array array = (Array) ob;
				int length = array.Length;
				Primitives.WritePrimitive(stream, (uint) length + 1);
				IStaticTypeSerializer typeSerializer = serializer.GetTypeData(elementType).TypeSerializer;

				foreach (object element in array)
				{
					typeSerializer.Serialize(serializer, elementType, stream, element);
				}
			}
		}

		public object Deserialize(Serializer serializer, Type staticType, Stream stream)
		{
			uint lengthField;
			Primitives.ReadPrimitive(stream, out lengthField);
			if (lengthField == 0)
			{
				return null;
			}
			else
			{
				uint length = lengthField - 1;
				Type elementType = staticType.GetElementType();
				IStaticTypeSerializer typeSerializer = serializer.GetTypeData(elementType).TypeSerializer;
				var array = Array.CreateInstance(elementType, length);
				for (int i =0; i < length; i++)
				{
					array.SetValue(typeSerializer.Deserialize(serializer, elementType, stream), i);
				}
				return array;
			}
		}
	}
}

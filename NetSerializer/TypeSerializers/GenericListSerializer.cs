/*
 * Copyright 2015 Tomi Valkeinen
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NetSerializer
{
	sealed class GenericListSerializer : IStaticTypeSerializer
	{
		private Dictionary<Type, ConstructorInfo> constructorCache = new Dictionary<Type, ConstructorInfo>();

		public bool Handles(Type type)
		{
			if (!type.IsGenericType)
				return false;

			return typeof(IList).IsAssignableFrom(type);
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			var genArgs = type.GetGenericArguments();

			return genArgs;
		}

		public void Serialize(Serializer serializer, Type staticType, Stream stream, object ob)
		{
			if (ob == null)
			{
				Primitives.WritePrimitive(stream, (uint) 0);
			}
			else
			{
				IList value = (IList) ob;
				int length = value.Count;
				Primitives.WritePrimitive(stream, (uint) length + 1);

				Type[] genericArguments = staticType.GetGenericArguments();
				Type elementType = genericArguments[0];
				TypeData elementTypeData = serializer.GetTypeData(elementType);

				foreach  (object element in value)
				{
					elementTypeData.TypeSerializer.Serialize(serializer, elementType, stream, element);
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
				int length = (int) lengthField - 1;
				Type[] genericArguments = staticType.GetGenericArguments();
				Type elementType = genericArguments[0];
				TypeData elementTypeData = serializer.GetTypeData(elementType);
				ConstructorInfo listConstructor = GetConstructorForType(staticType);
				IList result = (IList) listConstructor.Invoke(new object[] {length});
				for (int i = 0; i < length; i++)
				{
					object element = elementTypeData.TypeSerializer.Deserialize(serializer, elementType, stream);
					result.Add(element);
				}
				return result;
			}
		}

		private ConstructorInfo GetConstructorForType(Type staticType)
		{
			if (!constructorCache.ContainsKey(staticType))
			{
				Type[] genericArguments = staticType.GetGenericArguments();
				ConstructorInfo listConstructor = typeof(List<>).MakeGenericType(genericArguments).GetConstructor(new Type[]{typeof(int)});
				constructorCache[staticType] = listConstructor;
			}
			return constructorCache[staticType];
		}
	}
}

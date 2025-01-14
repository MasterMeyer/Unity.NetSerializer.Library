﻿/*
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
	sealed class DictionarySerializer : IStaticTypeSerializer
	{
		private Dictionary<Type, ConstructorInfo> constructorCache = new Dictionary<Type, ConstructorInfo>();

		public bool Handles(Type type)
		{
			if (!type.IsGenericType)
				return false;

			var genTypeDef = type.GetGenericTypeDefinition();

			return genTypeDef == typeof(Dictionary<,>);
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			// Dictionary<K,V> is stored as [Key, Value]+

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
				IDictionary valueDict = (IDictionary) ob;
				int length = valueDict.Count;
				Primitives.WritePrimitive(stream, (uint) length + 1);

				Type[] genericArguments = staticType.GetGenericArguments();
				Type keyType = genericArguments[0];
				Type valueType = genericArguments[1];
				TypeData keyTypeData = serializer.GetTypeData(keyType);
				TypeData valueTypeData = serializer.GetTypeData(valueType);

				foreach  (DictionaryEntry entry in valueDict)
				{
					keyTypeData.TypeSerializer.Serialize(serializer, keyType, stream, entry.Key);
					valueTypeData.TypeSerializer.Serialize(serializer, valueType, stream, entry.Value);
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
				Type keyType = genericArguments[0];
				Type valueType = genericArguments[1];
				TypeData keyTypeData = serializer.GetTypeData(keyType);
				TypeData valueTypeData = serializer.GetTypeData(valueType);
				ConstructorInfo dictionaryConstructor = GetConstructorForType(staticType);
				IDictionary result = (IDictionary) dictionaryConstructor.Invoke(new object[] {length});
				for (int i = 0; i < length; i++)
				{
					object key = keyTypeData.TypeSerializer.Deserialize(serializer, keyType, stream);
					object value = valueTypeData.TypeSerializer.Deserialize(serializer, valueType, stream);
					result.Add(key, value);
				}
				return result;
			}
		}

		private ConstructorInfo GetConstructorForType(Type staticType)
		{
			if (!constructorCache.ContainsKey(staticType))
			{
				Type[] genericArguments = staticType.GetGenericArguments();
				ConstructorInfo dictionaryConstructor =
					typeof(Dictionary<,>).MakeGenericType(genericArguments).GetConstructor(new Type[] {typeof(int)});
				constructorCache[staticType] = dictionaryConstructor;
			}
			return constructorCache[staticType];
		}
	}
}

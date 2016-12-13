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
using System.Reflection;

namespace NetSerializer
{
	sealed class ObjectSerializer : IStaticTypeSerializer
	{
		private Dictionary<Type, ConstructorInfo> constructorCache = new Dictionary<Type, ConstructorInfo>();
		public static Type OBJECT_TYPE = typeof(object);

		public bool Handles(Type type)
		{
			return OBJECT_TYPE.IsAssignableFrom(type);
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			var fields = Helpers.GetFieldInfos(type);

			foreach (var field in fields)
				yield return field.FieldType;
		}


		public void Serialize(Serializer serializer, Type staticType, Stream stream, object ob)
		{
			if (ob == null)
			{
				Primitives.WritePrimitive(stream, (uint)0);
				return;
			}

			var type = ob.GetType();

			TypeData typeData = serializer.GetTypeData(type);

			var id = typeData.TypeID;

			Primitives.WritePrimitive(stream, id);

			if (id == Serializer.ObjectTypeId)
				return;

			var fields = Helpers.GetFieldInfos(type);

			foreach (FieldInfo fieldInfo in fields)
			{
				object value = fieldInfo.GetValue(ob);
				Type fieldType = fieldInfo.FieldType;
				TypeData subTypeData = serializer.GetTypeData(fieldType);
				subTypeData.TypeSerializer.Serialize(serializer, fieldType, stream, value);
			}
		}

		public object Deserialize(Serializer serializer, Type staticType, Stream stream)
		{
			uint id;

			Primitives.ReadPrimitive(stream, out id);

			if (id == 0)
			{
				return null;
			}
			if (id == Serializer.ObjectTypeId)
			{
				return new object();
			}

			TypeData typeData = serializer.GetTypeDataById(id);
			Type type = typeData.Type;
			object result = GetConstructorForType(type).Invoke(new object[0]);
			var fields = Helpers.GetFieldInfos(type);
			foreach (FieldInfo fieldInfo in fields)
			{
				Type fieldType = fieldInfo.FieldType;
				TypeData subTypeData = serializer.GetTypeData(fieldType);
				object value = subTypeData.TypeSerializer.Deserialize(serializer, fieldType, stream);
				fieldInfo.SetValue(result, value);
			}
			return result;
		}

		private ConstructorInfo GetConstructorForType(Type type)
		{
			if (!constructorCache.ContainsKey(type))
			{
				ConstructorInfo objectConstructor = type.GetConstructor(new Type[0]);
				constructorCache[type] = objectConstructor;
			}
			return constructorCache[type];
		}
	}
}

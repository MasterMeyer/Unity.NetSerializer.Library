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

namespace NetSerializer
{
	public class Serializer
	{
		private long internalTypeCycles = 0;

		readonly static IStaticTypeSerializer[] s_typeSerializers = new IStaticTypeSerializer[] {
			new StringSerializer(),
			new BoolSerializer(),
			new IntSerializer(),
			new UIntSerializer(),
			new LongSerializer(),
			new DoubleSerializer(),
			new PrimitivesSerializer(),
			new ArraySerializer(),
			new EnumSerializer(),
			new DictionarySerializer(),
			new GenericListSerializer(),
			new ObjectSerializer(),
		};

		/// <summary>
		/// Initialize NetSerializer
		/// </summary>
		/// <param name="rootTypes">Types to be (de)serialized</param>
		public Serializer(IEnumerable<Type> rootTypes)
			: this(rootTypes, new Settings())
		{
		}

		/// <summary>
		/// Initialize NetSerializer
		/// </summary>
		/// <param name="rootTypes">Types to be (de)serialized</param>
		/// <param name="settings">Settings</param>
		public Serializer(IEnumerable<Type> rootTypes, Settings settings)
		{
			this.Settings = settings;

			lock (m_modifyLock)
			{
				m_runtimeTypeMap = new TypeDictionary();
				m_runtimeTypeIDList = new TypeIDList();

				AddTypesInternal(new Dictionary<Type, uint>()
				{
					{ typeof(object), Serializer.ObjectTypeId }
				});

				AddTypesInternal(rootTypes);
			}
		}

		/// <summary>
		/// Initialize NetSerializer
		/// </summary>
		/// <param name="typeMap">Type -> typeID map</param>
		public Serializer(Dictionary<Type, uint> typeMap)
			: this(typeMap, new Settings())
		{
		}

		/// <summary>
		/// Initialize NetSerializer
		/// </summary>
		/// <param name="typeMap">Type -> typeID map</param>
		/// <param name="settings">Settings</param>
		public Serializer(Dictionary<Type, uint> typeMap, Settings settings)
		{
			this.Settings = settings;

			lock (m_modifyLock)
			{
				m_runtimeTypeMap = new TypeDictionary();
				m_runtimeTypeIDList = new TypeIDList();

				AddTypesInternal(new Dictionary<Type, uint>()
				{
					{ typeof(object), Serializer.ObjectTypeId }
				});

				AddTypesInternal(typeMap);
			}
		}

		Dictionary<Type, uint> AddTypesInternal(IEnumerable<Type> roots)
		{
			var stack = new Stack<Type>(roots);
			var addedMap = new Dictionary<Type, uint>();

			while (stack.Count > 0)
			{
				internalTypeCycles++;
				var type = stack.Pop();

				if (m_runtimeTypeMap.ContainsKey(type))
					continue;

				if (type.ContainsGenericParameters)
					throw new NotSupportedException(String.Format("Type {0} contains generic parameters", type.FullName));

				while (m_runtimeTypeIDList.ContainsTypeID(m_nextAvailableTypeID))
					m_nextAvailableTypeID++;

				uint typeID = m_nextAvailableTypeID++;

				IStaticTypeSerializer serializer = GetTypeSerializer(type);

				var data = new TypeData(type, typeID, serializer);
				m_runtimeTypeMap[type] = data;
				m_runtimeTypeIDList[typeID] = data;

				addedMap[type] = typeID;

				foreach (var t in serializer.GetSubtypes(type))
				{
					internalTypeCycles++;
					if (m_runtimeTypeMap.ContainsKey(t) == false)
						stack.Push(t);
				}
			}

			return addedMap;
		}

		void AddTypesInternal(Dictionary<Type, uint> typeMap)
		{
			foreach (var kvp in typeMap)
			{
				internalTypeCycles++;
				var type = kvp.Key;
				uint typeID = kvp.Value;

				if (type == null)
					throw new ArgumentException("Null type in dictionary");

				if (typeID == 0)
					throw new ArgumentException("TypeID 0 is reserved");

				if (m_runtimeTypeMap.ContainsKey(type))
				{
					if (m_runtimeTypeMap[type].TypeID != typeID)
						throw new ArgumentException(String.Format("Type {0} already added with different TypeID", type.FullName));

					continue;
				}

				if (m_runtimeTypeIDList.ContainsTypeID(typeID))
					throw new ArgumentException(String.Format("Type with typeID {0} already added", typeID));

				if (type.IsAbstract || type.IsInterface)
					throw new ArgumentException(String.Format("Type {0} is abstract or interface", type.FullName));

				if (type.ContainsGenericParameters)
					throw new NotSupportedException(String.Format("Type {0} contains generic parameters", type.FullName));

				IStaticTypeSerializer serializer = GetTypeSerializer(type);

				var data = new TypeData(type, typeID, serializer);
				m_runtimeTypeMap[type] = data;
				m_runtimeTypeIDList[typeID] = data;
			}
		}


		/// <summary>
		/// Add rootTypes and all their subtypes, and return a mapping of all added types to typeIDs
		/// </summary>
		public Dictionary<Type, uint> AddTypes(IEnumerable<Type> rootTypes)
		{
			lock (m_modifyLock)
			{
				return AddTypesInternal(rootTypes);
			}
		}

		/// <summary>
		/// Add types obtained by a call to AddTypes in another Serializer instance
		/// </summary>
		public void AddTypes(Dictionary<Type, uint> typeMap)
		{
			lock (m_modifyLock)
				AddTypesInternal(typeMap);
		}

		/// <summary>
		/// Get SHA256 of the serializer type data. The SHA includes TypeIDs and Type's full names.
		/// The SHA can be used as a relatively good check to verify that two serializers
		/// (e.g. client and server) have the same type data.
		/// </summary>
		public string GetSHA256()
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				lock (m_modifyLock)
				{
					foreach (var item in m_runtimeTypeIDList.ToSortedList())
					{
						writer.Write(item.Key);
						writer.Write(item.Value.FullName);
					}
					// append number of cycles of internal parsing to get see when a parameter of a type is missing
					writer.Write(internalTypeCycles);
				}
				writer.Flush();
				stream.Flush();
				stream.Position = 0;
				var sha256 = System.Security.Cryptography.SHA256.Create();
				var bytes = sha256.ComputeHash(stream);

				var sb = new System.Text.StringBuilder();
				foreach (byte b in bytes)
					sb.Append(b.ToString("x2"));
				return sb.ToString();
			}
		}

		readonly TypeDictionary m_runtimeTypeMap;
		readonly TypeIDList m_runtimeTypeIDList;

		readonly object m_modifyLock = new object();

		uint m_nextAvailableTypeID = 1;

		internal const uint ObjectTypeId = 1;

		internal readonly Settings Settings = new Settings();


		public void Serialize<T>(Stream stream, T value)
		{
			Serialize(stream, value, typeof(T));
		}

		public void Serialize(Stream stream, object value, Type type)
		{
			IStaticTypeSerializer serializer = GetTypeSerializer(type);

			serializer.Serialize(this, type, stream, value);
		}

		public T Deserialize<T>(Stream stream)
		{
			return  (T) Deserialize(stream, typeof(T));
		}

		public object Deserialize(Stream stream, Type type)
		{
			IStaticTypeSerializer serializer = GetTypeSerializer(type);

			return serializer.Deserialize(this, type, stream);
		}

		internal TypeData GetTypeData(Type type)
		{
			return m_runtimeTypeMap[type];
		}

		internal TypeData GetTypeDataById(uint typeId)
		{
			return m_runtimeTypeIDList[typeId];
		}

		IStaticTypeSerializer GetTypeSerializer(Type type)
		{
			var serializer = this.Settings.CustomTypeSerializers.FirstOrDefault(h => h.Handles(type));

			if (serializer == null)
				serializer = s_typeSerializers.FirstOrDefault(h => h.Handles(type));

			if (serializer == null)
				throw new NotSupportedException(String.Format("No serializer for {0}", type.FullName));

			return serializer;
		}

	}
}

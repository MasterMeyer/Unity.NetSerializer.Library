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
	sealed class EnumSerializer : IStaticTypeSerializer
	{
		public bool Handles(Type type)
		{
			return type.IsEnum;
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			var underlyingType = Enum.GetUnderlyingType(type);

			return new[] { underlyingType };
		}

		public void Serialize(Serializer serializer, Type staticType, Stream stream, object ob)
		{
			var underlyingType = Enum.GetUnderlyingType(staticType);
			IStaticTypeSerializer primitiveSerializer = serializer.GetTypeData(underlyingType).TypeSerializer;
			primitiveSerializer.Serialize(serializer, underlyingType, stream, ob);
		}

		public object Deserialize(Serializer serializer, Type staticType, Stream stream)
		{
			var underlyingType = Enum.GetUnderlyingType(staticType);
			IStaticTypeSerializer primitiveSerializer = serializer.GetTypeData(underlyingType).TypeSerializer;
			return primitiveSerializer.Deserialize(serializer, underlyingType, stream);
		}
	}
}

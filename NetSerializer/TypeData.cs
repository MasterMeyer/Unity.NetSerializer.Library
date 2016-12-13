/*
 * Copyright 2015 Tomi Valkeinen
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetSerializer
{
	public sealed class TypeData
	{
		public TypeData(Type type, uint typeID, IStaticTypeSerializer typeSerializer)
		{
			this.Type = type;
			this.TypeID = typeID;
			this.TypeSerializer = typeSerializer;
		}

		public Type Type { get; private set; }
		public uint TypeID { get; private set; }

		public IStaticTypeSerializer TypeSerializer { get; private set; }
	}
}
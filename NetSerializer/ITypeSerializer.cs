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
	public interface IStaticTypeSerializer
	{
		/// <summary>
		/// Returns if this TypeSerializer handles the given type
		/// </summary>
		bool Handles(Type type);

		/// <summary>
		/// Return types that are needed to serialize the given type
		/// </summary>
		IEnumerable<Type> GetSubtypes(Type type);

		void Serialize(Serializer serializer, Type staticType, Stream stream, object ob);

		object Deserialize(Serializer serializer, Type staticType, Stream stream);
	}
}

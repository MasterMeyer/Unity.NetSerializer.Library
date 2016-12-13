/*
 * Copyright 2015 Tomi Valkeinen
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetSerializer
{
	static class Helpers
	{
		private static Dictionary<Type, IEnumerable<FieldInfo>> allFieldInfos = new Dictionary<Type, IEnumerable<FieldInfo>>();
		public static IEnumerable<FieldInfo> GetFieldInfos(Type type)
		{
			if (!allFieldInfos.ContainsKey(type))
			{
				var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
				                            BindingFlags.DeclaredOnly)
					.Where(fi => (fi.Attributes & FieldAttributes.NotSerialized) == 0)
					.OrderBy(f => f.Name, StringComparer.Ordinal);

				if (type.BaseType == null)
				{
					allFieldInfos[type] = new List<FieldInfo>(fields);
				}
				else
				{
					var baseFields = GetFieldInfos(type.BaseType);
					allFieldInfos[type] = new List<FieldInfo>(baseFields.Concat(fields));
				}
			}
			return allFieldInfos[type];
		}

	}
}

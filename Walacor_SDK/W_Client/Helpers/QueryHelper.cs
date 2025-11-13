// Copyright 2025 Walacor Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Walacor_SDK.W_Client.Helpers
{
    internal static class QueryHelper
    {
        public static IDictionary<string, string> BuildQueryFromObject(object? obj)
        {
            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
            if (obj == null)
            {
                return dict;
            }

            foreach (var p in obj.GetType().GetRuntimeProperties())
            {
                if (!p.CanRead)
                {
                    continue;
                }

                var val = p.GetValue(obj, null);
                if (val == null)
                {
                    continue;
                }

                string s;
                switch (val)
                {
                    case bool b:
                        s = b ? "true" : "false";
                        break;
                    case IFormattable fmt:
                        s = fmt.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;
                        break;
                    default:
                        s = val.ToString() ?? string.Empty;
                        break;
                }

                if (s.Length > 0)
                {
                    dict[p.Name] = s;
                }
            }

            return dict;
        }
    }
}

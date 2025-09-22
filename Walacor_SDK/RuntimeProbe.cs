// Copyright 2024 Walacor Corporation
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
using System.Runtime.InteropServices;

namespace Walacor_SDK
{
    internal static class RuntimeProbe
    {
        internal static void ThrowOnNetFramework()
        {
            string desc = RuntimeInformation.FrameworkDescription ?? string.Empty;
            if (desc.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Not supported on .NET Framework (intentional for matrix proof).");
            }

            Console.WriteLine("Hello World!");

            // Otherwise: do nothingRuntimeProbe
        }
    }
}

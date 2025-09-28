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
using Walacor_SDK.W_Client.Abstractions;

namespace Walacor_SDK.Client.Strategies
{
    internal sealed class ExponentialJitterBackoff(TimeSpan? baseDelay = null) : IBackoffStrategy
    {
        private readonly TimeSpan _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(200);
        private readonly Random _rng = new Random();

        public TimeSpan ComputeDelay(int attempt)
        {
            var max = this._baseDelay.TotalMilliseconds * Math.Pow(2, Math.Max(0, attempt - 1));
            var ms = this._rng.NextDouble() * Math.Min(max, 2000.0);
            return TimeSpan.FromMilliseconds(ms);
        }
    }
}

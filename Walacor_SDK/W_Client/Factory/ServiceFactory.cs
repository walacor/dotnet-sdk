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
using System.Collections.Concurrent;
using System.Threading;
using Walacor_SDK.W_Client.Constants;
using Walacor_SDK.W_Client.Context;

namespace Walacor_SDK.W_Client.Factory
{
    internal sealed class ServiceFactory
    {
        private readonly ClientContext _context;
        private readonly ConcurrentDictionary<Type, Lazy<object>> _singletons = new();

        public ServiceFactory(ClientContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public TService Get<TService>(Func<ClientContext, TService> factory)
            where TService : class
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var lazy = this._singletons.GetOrAdd(
                typeof(TService),
                _ => new Lazy<object>(
                    () =>
                    {
                        var instance = factory(this._context);
                        if (instance is null)
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    ExceptionMessages.FactoryReturnedNullForService,
                                    typeof(TService).Name));
                        }

                        return instance;
                    },
                    LazyThreadSafetyMode.ExecutionAndPublication));

            return (TService)lazy.Value;
        }
    }
}

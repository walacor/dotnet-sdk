using System;
using Walacor_SDK.W_Client.Abstractions;

namespace Test_Walacor_SDK.W_Client.Helpers
{
    public sealed class DeterministicBackoff : IBackoffStrategy
    {
        public int ComputeCalls { get; private set; }
        public TimeSpan ComputeDelay(int attempt)
        {
            ComputeCalls++;
            return TimeSpan.Zero;
        }
    }
}

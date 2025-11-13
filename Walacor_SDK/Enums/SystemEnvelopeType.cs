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
using System.Text;

namespace Walacor_SDK.Enums
{
    /// <summary>
    /// Defines system-level envelope types used within the platform.
    /// </summary>
    public enum SystemEnvelopeType : int
    {
        /// <summary>
        /// Represents an organization identifier envelope.
        /// </summary>
        OrgId = 5,

        /// <summary>
        /// Represents a user envelope.
        /// </summary>
        User = 10,

        /// <summary>
        /// Represents a user address envelope.
        /// Note: Shares value 11 with BPMCodeShare.
        /// </summary>
        UserAddress = 11,

        /// <summary>
        /// Represents a role envelope.
        /// </summary>
        Role = 15,

        /// <summary>
        /// Represents a mapping between a user and a role.
        /// </summary>
        UserRole = 16,

        /// <summary>
        /// Represents a storage location envelope.
        /// </summary>
        StorageLocation = 40,

        /// <summary>
        /// Represents scheduled job configuration.
        /// </summary>
        ScheduleJobs = 41,

        /// <summary>
        /// Represents a hashing signature envelope.
        /// </summary>
        HashingSignature = 42,

        /// <summary>
        /// Represents a schema definition envelope.
        /// </summary>
        Schema = 50,

        /// <summary>
        /// Represents a BPM (Business Process Management) action envelope.
        /// </summary>
        BPMAction = 51,

        /// <summary>
        /// Represents a BPM code or script envelope.
        /// </summary>
        BPMCode = 100,

        /// <summary>
        /// Represents a BPM approval action envelope.
        /// </summary>
        BMPApproval = 105,

        /// <summary>
        /// Represents sharing of BPM code.
        /// Note: Shares value 11 with UserAddress.
        /// </summary>
        BPMCodeShare = 11,
    }
}

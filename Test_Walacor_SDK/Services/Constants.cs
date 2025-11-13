using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Walacor_SDK.Services
{
    public class Constants
    {
        public const string SampleJson =
            "{\"success\":true,\"data\":[{\"Name\":\"INTEGER\",\"DefaultValue\":10,\"MinValue\":10,\"MaxValue\":10000},{\"Name\":\"TEXT\",\"DefaultValue\":\"\",\"MinLength\":1,\"MaxLength\":1048576},{\"Name\":\"DECIMAL\",\"DefaultValue\":0,\"MinValue\":1,\"MaxValue\":10000},{\"Name\":\"BOOLEAN\",\"DefaultValue\":false},{\"Name\":\"DATETIME(EPOCH)\",\"DefaultValue\":null},{\"Name\":\"ARRAY\",\"Type\":\"TEXT\"},{\"Name\":\"CRON\",\"DefaultValue\":\"*****\",\"MinLength\":5,\"MaxLength\":40}]}";
    }
}

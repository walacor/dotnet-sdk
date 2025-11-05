using Newtonsoft.Json;
using System.Collections.Generic;
using Walacor_SDK.Models.Results;
using Walacor_SDK.Models.Schema.Response;
using Walacor_SDK.W_Client.Mappers;
using Xunit;

namespace Test_Walacor_SDK.Services
{
    public class SchemaTests
    {
        [Fact]
        public void FromEnvlope_Unwraps_BaseResponse_And_Returns_Success()
        {
            var r = ResponseMapper.FromSuccessEnvelope<List<DataTypeDto>>(
                Constants.SampleJson,
                s => JsonConvert.DeserializeObject<BaseResponse<List<DataTypeDto>>>(s),
                statusCode: 200,
                correlationId: "corr-1");

            Assert.True(r.IsSuccess);
            Assert.NotNull(r.Value);
            Assert.Equal(7, r.Value!.Count);
            Assert.Equal("INTEGER", r.Value[0].Name);
            Assert.Equal(200, r.StatusCode);
            Assert.Equal("corr-1", r.CorrelationId);
        }

    }
}

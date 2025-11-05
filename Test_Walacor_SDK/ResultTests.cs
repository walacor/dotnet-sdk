using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;
using Xunit;

namespace Test_Walacor_SDK
{
    public class ResultTests
    {
        [Fact]
        public void Success_CarriesValue_AndMetadata()
        {
            var result = Result<string>.Success("ok", statusCode: 200, correlationId: "corr-123");

            Assert.True(result.IsSuccess);
            Assert.Equal("ok", result.Value);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("corr-123", result.CorrelationId);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Fail_CarriesError_AndMetadata()
        {
            var err = Error.Validation("bad_request", "The request is invalid.");
            var result = Result<string>.Fail(err, statusCode: 400, correlationId: "corr-400");


            Assert.False(result.IsSuccess);
            Assert.Same(err, result.Error);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("corr-400", result.CorrelationId);
        }
        [Fact]
        public void Map_PreservesMetadata_OnSuccess()
        {
            var ok = Result<string>.Success("abc", 200, "corr-xyz");

            var mapped = ok.Map(s => s.Length);

            Assert.True(mapped.IsSuccess);
            Assert.Equal(3, mapped.Value);
            Assert.Equal(200, mapped.StatusCode);
            Assert.Equal("corr-xyz", mapped.CorrelationId);
            Assert.Null(mapped.Error);
        }

        [Fact]
        public void Map_PreservesError_AndMetadata_OnFailure()
        {
            var err = Error.Server("boom");
            var fail = Result<string>.Fail(err, 500, "corr-500");

            var mapped = fail.Map(_ => 42);

            Assert.False(mapped.IsSuccess);
            Assert.Same(err, mapped.Error);
            Assert.Equal(500, mapped.StatusCode);
            Assert.Equal("corr-500", mapped.CorrelationId);
        }

        [Fact]
        public void Ensure_Passes_WhenPredicateTrue()
        {

        }
    }
}

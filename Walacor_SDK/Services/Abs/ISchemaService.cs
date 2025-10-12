using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Walacor_SDK.Services.Abs
{
    internal interface ISchemaService
    {

        Task<IAsyncResult<T>> GetDataTypes<T>();
    }
}

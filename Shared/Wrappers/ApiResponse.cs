using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Wrappers
{
    public class ApiResponse<T>
    {
        public T? Data { get; init; }
        public ApiError? Error { get; init; }

        public bool IsSuccess => Error == null;
    }
}
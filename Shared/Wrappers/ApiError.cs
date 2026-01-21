using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Wrappers
{
    public class ApiError
    {
        public string Code { get; init; } = default!;
        public string Message { get; init; } = default!;
    }
}

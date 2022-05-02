using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiPlugin.Features
{
    public abstract class Result<T>
    {
        protected Result(T? result, bool success = true, string message = "Success")
        {
            Success = success;
            Message = message;
            Data = result;
        }

        public T? Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class QueryResult<T> : Result<T>
    {
        public QueryResult(T? result, bool success = true, string message = "Success") : base(result, success, message)
        {
        }
    }
}

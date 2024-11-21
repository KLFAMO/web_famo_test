using System.Net;

namespace FamoNET.Model
{
    public class ApiResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
    }
}

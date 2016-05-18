using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Samples.TodoList.WebApi.Controllers
{
    [RoutePrefix("api/commands")]
    public class CommandsController : ApiController
    {
        [HttpGet, Route("new-ids")]
        public string[] GetNewIds(int count)
        {
            if (count < 1) count = 1;
            if (count > 100) count = 100;

            var result = Enumerable
                .Range(0, count)
                .Select(i => Guid.NewGuid().ToString("D"))
                .ToArray();

            return result;
        }

        [HttpPost, Route("{aggregate}/{id}/{command}")]
        public async Task<IHttpActionResult> Post(string aggregate, string id, string command)
        {
            return Ok(1);
        }
    }
}
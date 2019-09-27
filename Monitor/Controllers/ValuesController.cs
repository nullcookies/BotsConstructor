using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLSharp.Core;
using Microsoft.AspNetCore.Mvc;

namespace Monitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private TestTelegramApi _testTelegramApi;

        public ValuesController(TestTelegramApi testTelegramApi)
        {
            _testTelegramApi = testTelegramApi;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var sendCodeAsync = _testTelegramApi.SendCodeAsync();
            return new string[] { "value1", "value2" };
        }

      
    

        // GET api/values/5
        [HttpGet("{code}")]
        public ActionResult<string> Get(string code)
        {
            var makeAuthAsync = _testTelegramApi.MakeAuthAsync(code);

            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dockerapi.Services;
using dockerapi.Utility;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace dockerapi.Controllers
{
    [Route("api/docker")]
    public class DockerController : Controller
    {
        private DockerApiService _service;

        public DockerController(DockerApiService service)
        {
            _service = service;
        }


        public RegistryCredential GetDockerCredential()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return null;
            }

            var header = Request.Headers["Authorization"];

            if (header.Count < 1)
            {
                return null;
            }

            if (!header[0].StartsWith("Basic "))
            {
                return null;
            }

            string basicAuth = header[0].Substring("Basic ".Length);

            if (!Request.Headers.ContainsKey("Registry"))
            {
                return null;
            }

            return new RegistryCredential() { BasicAuth = basicAuth, Registry = Request.Headers["Registry"] };
        }

        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            RegistryCredential cred = GetDockerCredential();
            if (cred == null)
            {
                return new UnauthorizedResult();
            }

            if (cred.Registry == null)
            {
                return new UnauthorizedResult();
            }

            if (!cred.Registry.Contains("."))
            {
                return new UnauthorizedResult();
            }

            if (await _service.TestCredentials(cred.Registry, cred.BasicAuth))
            {
                return new OkResult();
            }

            return new UnauthorizedResult();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

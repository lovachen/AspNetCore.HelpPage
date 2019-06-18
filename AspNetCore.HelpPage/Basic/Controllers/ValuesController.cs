using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    /// <summary>
    /// values控制器的说明内容
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// 获取数组集合
        /// </summary>
        /// <returns></returns>
        // GET: api/Values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Values/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("login")]
        [ProducesResponseType(typeof(ResModel<TestData>),StatusCodes.Status200OK)]
        public IActionResult Login([FromBody]LoginModel model)
        {
            ResModel<TestData> s = new ResModel<TestData>();
            s.data = new TestData() { token = "badsfsf" };
            return Ok(s);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mobile">手机号码</param>
        /// <returns></returns>
        [HttpGet("salts")]
        public IActionResult GetSalt([FromQuery]string mobile)
        {
            ResModel<object> res = new ResModel<object>();
            res.data = new { slat ="112321312312"};
            return Ok(res);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>登录成功，返回 code:0 成功其它请看msg提示</returns>
        [HttpGet("getlogin")]
        [ProducesResponseType(typeof(ResModel<TestData>),StatusCodes.Status200OK)]
        public IActionResult GetLogin([FromQuery] LoginModel model)
        {
            ResModel<TestData> s = new ResModel<TestData>();
            s.data = new TestData() { token = "badsfsf" };
            return Ok();
        }
    }
}

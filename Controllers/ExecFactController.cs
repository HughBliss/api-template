
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Study.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecFactController : ControllerBase
    {
        [HttpGet("{num}")]
        public async Task<ActionResult<string>> Index(int num)
        {
            return Ok(Fact(num).ToString() + " result");
        }

        BigInteger Fact(BigInteger num) {
            var result = new BigInteger(1);
            for (var i = 1; i <= num; i++) {
                result *= i;
            }
            return result;
        }
    }
}

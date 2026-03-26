using Anir.Data;
using Anir.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Anir.Api.Controllers
{
    [ApiController]
    [Route("api/provinces")]
    public class ProvincesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProvincesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/provinces
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProvinceDto>>> GetAll()
        {
            var provinces = await _context.Provinces
                .Select(p => new ProvinceDto
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return Ok(provinces);
        }
    }
}

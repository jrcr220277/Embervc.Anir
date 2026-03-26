using Anir.Data;
using Anir.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Anir.Api.Controllers
{
    [ApiController]
    [Route("api/municipalities")]
    public class MunicipalitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MunicipalitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/municipalities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MunicipalityDto>>> GetAll()
        {
            var municipalities = await _context.Municipalities
                .Select(m => new MunicipalityDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ProvinceId = m.ProvinceId,
                    IsProvinceCapital = m.IsProvinceCapital
                })
                .ToListAsync();

            return Ok(municipalities);
        }

        // GET: api/municipalities/by-province/5
        [HttpGet("by-province/{provinceId:int}")]
        public async Task<ActionResult<IEnumerable<MunicipalityDto>>> GetByProvince(int provinceId)
        {
            var municipalities = await _context.Municipalities
                .Where(m => m.ProvinceId == provinceId)
                .Select(m => new MunicipalityDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ProvinceId = m.ProvinceId,
                    IsProvinceCapital = m.IsProvinceCapital
                })
                .ToListAsync();

            return Ok(municipalities);
        }
    }
}

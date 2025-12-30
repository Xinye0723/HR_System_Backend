using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HR_System_API.Models;

namespace HR_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeInfosController : ControllerBase
    {
        private readonly HrContext _context;

        public EmployeeInfosController(HrContext context)
        {
            _context = context;
        }

        // GET: api/EmployeeInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeInfo>>> GetEmployeeInfos()
        {
            return await _context.EmployeeInfos.ToListAsync();
        }

        // GET: api/EmployeeInfos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeInfo>> GetEmployeeInfo(string id)
        {
            var employeeInfo = await _context.EmployeeInfos.FindAsync(id);

            if (employeeInfo == null)
            {
                return NotFound();
            }

            return employeeInfo;
        }

        // PUT: api/EmployeeInfos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployeeInfo(string id, EmployeeInfo employeeInfo)
        {
            if (id != employeeInfo.EmployeeId)
            {
                return BadRequest();
            }

            _context.Entry(employeeInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/EmployeeInfos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmployeeInfo>> PostEmployeeInfo(EmployeeInfo employeeInfo)
        {
            _context.EmployeeInfos.Add(employeeInfo);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EmployeeInfoExists(employeeInfo.EmployeeId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEmployeeInfo", new { id = employeeInfo.EmployeeId }, employeeInfo);
        }

        // DELETE: api/EmployeeInfos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeInfo(string id)
        {
            var employeeInfo = await _context.EmployeeInfos.FindAsync(id);
            if (employeeInfo == null)
            {
                return NotFound();
            }

            _context.EmployeeInfos.Remove(employeeInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeInfoExists(string id)
        {
            return _context.EmployeeInfos.Any(e => e.EmployeeId == id);
        }
    }
}

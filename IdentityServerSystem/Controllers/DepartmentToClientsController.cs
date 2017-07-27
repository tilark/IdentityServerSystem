using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityServerSystem.Data;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IdentityServerSystem.Controllers
{
    [Route("departments")]
    [Authorize]

    public class DepartmentToClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentToClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /<controller>/
        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(from a in _context.ApplicationDepartments select new {  a.DepartmentID,  a.DepartmentName });
        }

    }
}

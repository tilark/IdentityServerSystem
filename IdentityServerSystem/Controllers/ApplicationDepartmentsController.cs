using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IdentityServerSystem.Data;
using IdentityServerSystem.Models;

namespace IdentityServerSystem.Controllers
{
    public class ApplicationDepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApplicationDepartmentsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: ApplicationDepartments1
        public async Task<IActionResult> Index()
        {
            return View(await _context.ApplicationDepartments.ToListAsync());
        }

        // GET: ApplicationDepartments1/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationDepartment = await _context.ApplicationDepartments
                .SingleOrDefaultAsync(m => m.DepartmentID == id);
            if (applicationDepartment == null)
            {
                return NotFound();
            }

            return View(applicationDepartment);
        }

        // GET: ApplicationDepartments1/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ApplicationDepartments1/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentID,DepartmentName,Remarks,TimesStamp")] ApplicationDepartment applicationDepartment)
        {
            if (ModelState.IsValid)
            {
                applicationDepartment.DepartmentID = Guid.NewGuid();
                _context.Add(applicationDepartment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(applicationDepartment);
        }

        // GET: ApplicationDepartments1/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationDepartment = await _context.ApplicationDepartments.AsNoTracking().SingleOrDefaultAsync(m => m.DepartmentID == id);
            if (applicationDepartment == null)
            {
                return NotFound();
            }
            return View(applicationDepartment);
        }

        // POST: ApplicationDepartments1/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, byte[] timesStamp)
        {
            if (id.Equals(Guid.Empty))
            {
                return NotFound();
            }
            var applicationDepartment = await _context.ApplicationDepartments.SingleOrDefaultAsync(m => m.DepartmentID == id);

            if(applicationDepartment == null)
            {
                var deletedApplicationDepartment = new ApplicationDepartment();
                await TryUpdateModelAsync(deletedApplicationDepartment);
                ModelState.AddModelError(string.Empty, "无法保存，原科室已经被删除！");
                return View(deletedApplicationDepartment);
            }
            _context.Entry(applicationDepartment).Property("TimesStamp").OriginalValue = timesStamp;
            if(await TryUpdateModelAsync<ApplicationDepartment>(applicationDepartment, "", s => s.DepartmentName,s=> s.Remarks))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch(DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (ApplicationDepartment)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if(databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                    "无法保存，原科室已经被删除！");
                    }
                    else
                    {
                        var databaseValues = (ApplicationDepartment)databaseEntry.ToObject();
                        if(databaseValues.DepartmentName != clientValues.DepartmentName)
                        {
                            ModelState.AddModelError("DepartmentName", $"当前值: {databaseValues.DepartmentName}");

                        }
                        if (databaseValues.Remarks != clientValues.Remarks)
                        {
                            ModelState.AddModelError("Remarks", $"当前值: {databaseValues.Remarks}");

                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                        + "was modified by another user after you got the original value. The "
                        + "edit operation was canceled and the current values in the database "
                        + "have been displayed. If you still want to edit this record, click "
                        + "the Save button again. Otherwise click the Back to List hyperlink.");
                        applicationDepartment.TimesStamp = (byte[])databaseValues.TimesStamp;
                        ModelState.Remove("TimesStamp");
                    }
                }
            }           
            return View(applicationDepartment);
        }

        // GET: ApplicationDepartments1/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationDepartment = await _context.ApplicationDepartments
                .SingleOrDefaultAsync(m => m.DepartmentID == id);
            if (applicationDepartment == null)
            {
                return NotFound();
            }

            return View(applicationDepartment);
        }

        // POST: ApplicationDepartments1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var applicationDepartment = await _context.ApplicationDepartments.SingleOrDefaultAsync(m => m.DepartmentID == id);
            _context.ApplicationDepartments.Remove(applicationDepartment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ApplicationDepartmentExists(Guid id)
        {
            return _context.ApplicationDepartments.Any(e => e.DepartmentID == id);
        }
    }
}

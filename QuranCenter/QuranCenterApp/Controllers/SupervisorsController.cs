using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCenterApp.Data;
using QuranCenterApp.Models;

namespace QuranCenterApp.Controllers;

public class SupervisorsController : Controller
{
    private readonly AppDbContext _db;
    public SupervisorsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var supervisors = await _db.Supervisors
            .Include(s => s.Person)
            .Include(s => s.Classrooms)
            .ToListAsync();
        return View(supervisors);
    }

    public IActionResult Create() => View();

    public async Task<IActionResult> Edit(int id)
    {
        var s = await _db.Supervisors.Include(x => x.Person).AsNoTracking().FirstOrDefaultAsync(x => x.SupervisorID == id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supervisor supervisor, Person person)
    {
        if (id != supervisor.SupervisorID) return BadRequest();
        ModelState.Remove("Person");
        ModelState.Remove("Classrooms");
        ModelState.Remove("ZipCodeNav");
        ModelState.Remove("PhoneNumbers");
        if (ModelState.IsValid)
        {
            try
            {
                person.ZipCode = string.IsNullOrWhiteSpace(person.ZipCode) ? null : person.ZipCode;
                person.Email   = string.IsNullOrWhiteSpace(person.Email)   ? null : person.Email;
                _db.Entry(person).State      = EntityState.Modified;
                _db.Entry(supervisor).State  = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["Success"] = "تم تحديث بيانات المشرف بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطأ في الحفظ: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        return View(supervisor);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Person person, Supervisor supervisor)
    {
        ModelState.Remove("Person");
        ModelState.Remove("Classrooms");
        ModelState.Remove("ZipCodeNav");
        ModelState.Remove("PhoneNumbers");

        if (ModelState.IsValid)
        {
            try
            {
                person.ZipCode = string.IsNullOrWhiteSpace(person.ZipCode) ? null : person.ZipCode;
                person.Email   = string.IsNullOrWhiteSpace(person.Email)   ? null : person.Email;
                _db.Persons.Add(person);
                await _db.SaveChangesAsync();
                supervisor.SupervisorID = person.PersonID;
                _db.Supervisors.Add(supervisor);
                await _db.SaveChangesAsync();
                TempData["Success"] = "تمت إضافة المشرف بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطأ في الحفظ: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        return View(person);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Supervisors.Include(x => x.Person).FirstOrDefaultAsync(x => x.SupervisorID == id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var p = await _db.Persons.FindAsync(id);
        if (p != null) { _db.Persons.Remove(p); await _db.SaveChangesAsync(); }
        TempData["Success"] = "تم حذف المشرف!";
        return RedirectToAction(nameof(Index));
    }
}

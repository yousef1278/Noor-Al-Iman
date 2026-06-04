using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCenterApp.Data;
using QuranCenterApp.Models;

namespace QuranCenterApp.Controllers;

public class TeachersController : Controller
{
    private readonly AppDbContext _db;

    public TeachersController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var teachers = await _db.Teachers
            .Include(t => t.Person)
            .Include(t => t.Classrooms)
            .OrderBy(t => t.Person.FirstName)
            .ToListAsync();
        return View(teachers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var teacher = await _db.Teachers
            .Include(t => t.Person).ThenInclude(p => p.PhoneNumbers)
            .Include(t => t.Classrooms).ThenInclude(c => c.Curriculum)
            .FirstOrDefaultAsync(t => t.TeacherID == id);

        if (teacher == null) return NotFound();
        return View(teacher);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Person person, Teacher teacher)
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
                teacher.TeacherID = person.PersonID;
                _db.Teachers.Add(teacher);
                await _db.SaveChangesAsync();
                TempData["Success"] = "تمت إضافة المعلم بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطأ في الحفظ: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        return View(person);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var teacher = await _db.Teachers
            .Include(t => t.Person)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TeacherID == id);

        if (teacher == null) return NotFound();
        return View(teacher);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Teacher teacher, Person person)
    {
        if (id != teacher.TeacherID) return BadRequest();

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

                _db.Entry(person).State  = EntityState.Modified;
                _db.Entry(teacher).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["Success"] = "تم تحديث بيانات المعلم!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطأ: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        return View(teacher);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var teacher = await _db.Teachers
            .Include(t => t.Person)
            .FirstOrDefaultAsync(t => t.TeacherID == id);

        if (teacher == null) return NotFound();
        return View(teacher);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var person = await _db.Persons.FindAsync(id);
        if (person != null)
        {
            _db.Persons.Remove(person);
            await _db.SaveChangesAsync();
        }
        TempData["Success"] = "Teacher deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}

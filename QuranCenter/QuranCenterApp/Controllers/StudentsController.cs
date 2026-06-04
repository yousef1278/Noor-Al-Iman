using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCenterApp.Data;
using QuranCenterApp.Models;

namespace QuranCenterApp.Controllers;

public class StudentsController : Controller
{
    private readonly AppDbContext _db;

    public StudentsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Students
            .Include(s => s.Person)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s =>
                s.Person.FirstName.Contains(search) ||
                s.Person.LastName.Contains(search)  ||
                (s.Person.Email != null && s.Person.Email.Contains(search)));

        ViewBag.Search = search;
        return View(await query.OrderBy(s => s.Person.FirstName).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var student = await _db.Students
            .Include(s => s.Person).ThenInclude(p => p.PhoneNumbers)
            .Include(s => s.Enrollments).ThenInclude(e => e.Classroom).ThenInclude(c => c.Curriculum)
            .Include(s => s.Memorizations)
            .Include(s => s.GiftDistributions).ThenInclude(gd => gd.Gift)
            .FirstOrDefaultAsync(s => s.StudentID == id);

        if (student == null) return NotFound();
        return View(student);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Person person, Student student)
    {
        ModelState.Remove("Student");
        ModelState.Remove("Person");
        ModelState.Remove("PhoneNumbers");
        ModelState.Remove("ZipCodeNav");

        if (ModelState.IsValid)
        {
            try
            {
                person.ZipCode = string.IsNullOrWhiteSpace(person.ZipCode) ? null : person.ZipCode;
                person.Email   = string.IsNullOrWhiteSpace(person.Email)   ? null : person.Email;
                _db.Persons.Add(person);
                await _db.SaveChangesAsync();
                student.StudentID = person.PersonID;
                _db.Students.Add(student);
                await _db.SaveChangesAsync();
                TempData["Success"] = "تمت إضافة الطالب بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطأ في الحفظ: " + ex.InnerException?.Message ?? ex.Message);
            }
        }
        return View(person);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var student = await _db.Students
            .Include(s => s.Person)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentID == id);

        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Student student, Person person)
    {
        if (id != student.StudentID) return BadRequest();

        ModelState.Remove("Person");
        ModelState.Remove("Enrollments");
        ModelState.Remove("Attendances");
        ModelState.Remove("Memorizations");
        ModelState.Remove("GiftDistributions");
        ModelState.Remove("ZipCodeNav");
        ModelState.Remove("PhoneNumbers");

        if (ModelState.IsValid)
        {
            try
            {
                person.ZipCode = string.IsNullOrWhiteSpace(person.ZipCode) ? null : person.ZipCode;
                person.Email   = string.IsNullOrWhiteSpace(person.Email)   ? null : person.Email;

                _db.Entry(person).State  = EntityState.Modified;
                _db.Entry(student).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["Success"] = "تم تحديث بيانات الطالب!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Students.Any(s => s.StudentID == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(student);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var student = await _db.Students
            .Include(s => s.Person)
            .FirstOrDefaultAsync(s => s.StudentID == id);

        if (student == null) return NotFound();
        return View(student);
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
        TempData["Success"] = "Student deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}

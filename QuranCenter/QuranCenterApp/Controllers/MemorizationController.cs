using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCenterApp.Data;
using QuranCenterApp.Models;

namespace QuranCenterApp.Controllers;

public class MemorizationController : Controller
{
    private readonly AppDbContext _db;

    public MemorizationController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? studentId)
    {
        var query = _db.Memorizations
            .Include(m => m.Student).ThenInclude(s => s.Person)
            .AsQueryable();

        if (studentId.HasValue) query = query.Where(m => m.StudentID == studentId);

        var students = await _db.Students
            .Include(s => s.Person)
            .Select(s => new { s.StudentID, Name = s.Person.FirstName + " " + s.Person.LastName })
            .ToListAsync();

        ViewBag.Students  = new SelectList(students, "StudentID", "Name", studentId);
        ViewBag.StudentId = studentId;

        return View(await query.OrderByDescending(m => m.DateCompleted).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await PopulateStudents();
        return View(new Memorization { DateCompleted = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Memorization memorization)
    {
        if (ModelState.IsValid)
        {
            _db.Memorizations.Add(memorization);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إضافة سجل الحفظ بنجاح!";
            return RedirectToAction(nameof(Index));
        }
        await PopulateStudents();
        return View(memorization);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var m = await _db.Memorizations.FindAsync(id);
        if (m == null) return NotFound();
        await PopulateStudents(m.StudentID);
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Memorization memorization)
    {
        if (id != memorization.MemorizationID) return BadRequest();
        if (ModelState.IsValid)
        {
            _db.Update(memorization);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم تحديث سجل الحفظ بنجاح!";
            return RedirectToAction(nameof(Index));
        }
        await PopulateStudents(memorization.StudentID);
        return View(memorization);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var m = await _db.Memorizations
            .Include(x => x.Student).ThenInclude(s => s.Person)
            .FirstOrDefaultAsync(x => x.MemorizationID == id);
        if (m == null) return NotFound();
        return View(m);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var m = await _db.Memorizations.FindAsync(id);
        if (m != null) { _db.Memorizations.Remove(m); await _db.SaveChangesAsync(); }
        TempData["Success"] = "تم حذف السجل بنجاح!";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateStudents(int? selected = null)
    {
        var students = await _db.Students
            .Include(s => s.Person)
            .Select(s => new { s.StudentID, Name = s.Person.FirstName + " " + s.Person.LastName })
            .ToListAsync();
        ViewBag.StudentID = new SelectList(students, "StudentID", "Name", selected);
    }
}

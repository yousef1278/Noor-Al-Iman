using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCenterApp.Data;
using QuranCenterApp.Models;

namespace QuranCenterApp.Controllers;

public class AttendanceController : Controller
{
    private readonly AppDbContext _db;

    public AttendanceController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? classroomId, DateTime? date)
    {
        var query = _db.Attendances
            .Include(a => a.Student).ThenInclude(s => s.Person)
            .Include(a => a.Classroom)
            .AsQueryable();

        if (classroomId.HasValue) query = query.Where(a => a.ClassroomID == classroomId);
        if (date.HasValue)        query = query.Where(a => a.AttendanceDate == date.Value);

        ViewBag.Classrooms   = new SelectList(await _db.Classrooms.ToListAsync(), "ClassroomID", "ClassroomName");
        ViewBag.ClassroomId  = classroomId;
        ViewBag.FilterDate   = date;

        return View(await query.OrderByDescending(a => a.AttendanceDate).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new Attendance { AttendanceDate = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Attendance attendance)
    {
        if (ModelState.IsValid)
        {
            _db.Attendances.Add(attendance);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم تسجيل الحضور بنجاح!";
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns();
        return View(attendance);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var a = await _db.Attendances.FindAsync(id);
        if (a == null) return NotFound();
        await PopulateDropdowns();
        return View(a);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Attendance attendance)
    {
        if (id != attendance.AttendanceID) return BadRequest();
        if (ModelState.IsValid)
        {
            _db.Update(attendance);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم تحديث سجل الحضور بنجاح!";
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns();
        return View(attendance);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Attendances
            .Include(x => x.Student).ThenInclude(s => s.Person)
            .Include(x => x.Classroom)
            .FirstOrDefaultAsync(x => x.AttendanceID == id);
        if (a == null) return NotFound();
        return View(a);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var a = await _db.Attendances.FindAsync(id);
        if (a != null) { _db.Attendances.Remove(a); await _db.SaveChangesAsync(); }
        TempData["Success"] = "تم حذف سجل الحضور بنجاح!";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns()
    {
        var students = await _db.Students
            .Include(s => s.Person)
            .Select(s => new { s.StudentID, Name = s.Person.FirstName + " " + s.Person.LastName })
            .ToListAsync();

        ViewBag.StudentID   = new SelectList(students,                    "StudentID",   "Name");
        ViewBag.ClassroomID = new SelectList(await _db.Classrooms.ToListAsync(), "ClassroomID", "ClassroomName");
        ViewBag.StatusList  = new SelectList(new[]
        {
            new { Value = "P", Text = "Present" },
            new { Value = "A", Text = "Absent"  },
            new { Value = "L", Text = "Late"    }
        }, "Value", "Text");
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCenterApp.Data;
using QuranCenterApp.Models;

namespace QuranCenterApp.Controllers;

public class ClassroomsController : Controller
{
    private readonly AppDbContext _db;

    public ClassroomsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var classrooms = await _db.Classrooms
            .Include(c => c.Curriculum)
            .Include(c => c.Teacher).ThenInclude(t => t.Person)
            .Include(c => c.Supervisor).ThenInclude(s => s!.Person)
            .Include(c => c.Enrollments)
            .ToListAsync();
        return View(classrooms);
    }

    public async Task<IActionResult> Details(int id)
    {
        var classroom = await _db.Classrooms
            .Include(c => c.Curriculum)
            .Include(c => c.Teacher).ThenInclude(t => t.Person)
            .Include(c => c.Supervisor).ThenInclude(s => s!.Person)
            .Include(c => c.Enrollments).ThenInclude(e => e.Student).ThenInclude(s => s.Person)
            .FirstOrDefaultAsync(c => c.ClassroomID == id);

        if (classroom == null) return NotFound();
        return View(classroom);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Classroom classroom)
    {
        ModelState.Remove("Curriculum");
        ModelState.Remove("Teacher");
        ModelState.Remove("Supervisor");
        ModelState.Remove("Enrollments");
        ModelState.Remove("Attendances");

        if (ModelState.IsValid)
        {
            try
            {
                _db.Classrooms.Add(classroom);
                await _db.SaveChangesAsync();
                TempData["Success"] = "تم إنشاء الفصل بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطأ: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        await PopulateDropdowns(classroom);
        return View(classroom);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var classroom = await _db.Classrooms.AsNoTracking().FirstOrDefaultAsync(c => c.ClassroomID == id);
        if (classroom == null) return NotFound();
        await PopulateDropdowns(classroom);
        return View(classroom);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Classroom classroom)
    {
        if (id != classroom.ClassroomID) return BadRequest();

        ModelState.Remove("Curriculum");
        ModelState.Remove("Teacher");
        ModelState.Remove("Supervisor");
        ModelState.Remove("Enrollments");
        ModelState.Remove("Attendances");

        if (ModelState.IsValid)
        {
            try
            {
                _db.Entry(classroom).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["Success"] = "تم تحديث الفصل بنجاح!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطأ: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        await PopulateDropdowns(classroom);
        return View(classroom);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var classroom = await _db.Classrooms
            .Include(c => c.Curriculum)
            .Include(c => c.Teacher).ThenInclude(t => t.Person)
            .FirstOrDefaultAsync(c => c.ClassroomID == id);

        if (classroom == null) return NotFound();
        return View(classroom);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var classroom = await _db.Classrooms
                .Include(c => c.Enrollments)
                .Include(c => c.Attendances)
                .FirstOrDefaultAsync(c => c.ClassroomID == id);

            if (classroom != null)
            {
                _db.Enrollments.RemoveRange(classroom.Enrollments);
                _db.Attendances.RemoveRange(classroom.Attendances);
                _db.Classrooms.Remove(classroom);
                await _db.SaveChangesAsync();
                TempData["Success"] = "تم حذف الفصل وجميع بياناته المرتبطة بنجاح!";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "تعذّر حذف الفصل: " + (ex.InnerException?.Message ?? ex.Message);
        }
        return RedirectToAction(nameof(Index));
    }

    // Level priority map
    private static readonly Dictionary<string, int> LevelRank = new()
    {
        { "Beginner",     1 },
        { "Intermediate", 2 },
        { "Advanced",     3 }
    };

    private static readonly Dictionary<string, string> LevelAr = new()
    {
        { "Beginner",     "مبتدئ" },
        { "Intermediate", "متوسط" },
        { "Advanced",     "متقدم" }
    };

    // ── ENROLL STUDENT ──────────────────────────────────────
    public async Task<IActionResult> Enroll(int id)
    {
        var classroom = await _db.Classrooms
            .Include(c => c.Enrollments)
            .Include(c => c.Curriculum)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClassroomID == id);

        if (classroom == null) return NotFound();

        var enrolledIds = classroom.Enrollments.Select(e => e.StudentID).ToList();

        // Filter: only show students whose level meets the classroom requirement
        int requiredRank = LevelRank.GetValueOrDefault(classroom.RequiredLevel, 1);

        var available = await _db.Students
            .Include(s => s.Person)
            .Where(s => !enrolledIds.Contains(s.StudentID))
            .ToListAsync();

        // Apply level check only — student level is the credential
        var eligible = available.Where(s =>
        {
            int studentRank = LevelRank.GetValueOrDefault(s.Level, 1);
            return studentRank >= requiredRank;
        }).Select(s => new {
            s.StudentID,
            Name = s.Person.FirstName + " " + s.Person.LastName + " (" + LevelAr.GetValueOrDefault(s.Level, s.Level) + ")"
        }).ToList();

        ViewBag.ClassroomID    = classroom.ClassroomID;
        ViewBag.ClassroomName  = classroom.ClassroomName;
        ViewBag.MaxSize        = classroom.MaxSize;
        ViewBag.CurrentCount   = enrolledIds.Count;
        ViewBag.RequiredLevel  = LevelAr.GetValueOrDefault(classroom.RequiredLevel, classroom.RequiredLevel);
        ViewBag.Students       = new SelectList(eligible, "StudentID", "Name");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int classroomId, int studentId)
    {
        var classroom = await _db.Classrooms
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassroomID == classroomId);

        if (classroom == null) return NotFound();

        if (classroom.Enrollments.Count >= classroom.MaxSize)
        {
            TempData["Error"] = "الفصل ممتلئ! لا يمكن إضافة المزيد من الطلاب.";
            return RedirectToAction(nameof(Details), new { id = classroomId });
        }

        var student = await _db.Students.FindAsync(studentId);
        if (student != null)
        {
            int studentRank  = LevelRank.GetValueOrDefault(student.Level, 1);
            int requiredRank = LevelRank.GetValueOrDefault(classroom.RequiredLevel, 1);

            if (studentRank < requiredRank)
            {
                TempData["Error"] = $"مستوى الطالب ({LevelAr.GetValueOrDefault(student.Level)}) لا يستوفي المتطلب ({LevelAr.GetValueOrDefault(classroom.RequiredLevel)}).";
                return RedirectToAction(nameof(Details), new { id = classroomId });
            }
        }

        bool alreadyEnrolled = await _db.Enrollments
            .AnyAsync(e => e.StudentID == studentId && e.ClassroomID == classroomId);

        if (alreadyEnrolled)
        {
            TempData["Error"] = "الطالب مسجّل بالفعل في هذا الفصل.";
            return RedirectToAction(nameof(Details), new { id = classroomId });
        }

        _db.Enrollments.Add(new Enrollment
        {
            StudentID      = studentId,
            ClassroomID    = classroomId,
            EnrollmentDate = DateTime.Today
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "تم تسجيل الطالب في الفصل بنجاح!";
        return RedirectToAction(nameof(Details), new { id = classroomId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int enrollmentId, int classroomId)
    {
        var enrollment = await _db.Enrollments.FindAsync(enrollmentId);
        if (enrollment != null)
        {
            _db.Enrollments.Remove(enrollment);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إلغاء تسجيل الطالب.";
        }
        return RedirectToAction(nameof(Details), new { id = classroomId });
    }

    private async Task PopulateDropdowns(Classroom? selected = null)
    {
        var teachers = await _db.Teachers
            .Include(t => t.Person)
            .Select(t => new { t.TeacherID, Name = t.Person.FirstName + " " + t.Person.LastName })
            .ToListAsync();

        var supervisors = await _db.Supervisors
            .Include(s => s.Person)
            .Select(s => new { s.SupervisorID, Name = s.Person.FirstName + " " + s.Person.LastName })
            .ToListAsync();

        var curricula = await _db.Curricula.ToListAsync();

        ViewBag.TeacherID    = new SelectList(teachers,    "TeacherID",    "Name",    selected?.TeacherID);
        ViewBag.SupervisorID = new SelectList(supervisors, "SupervisorID", "Name",    selected?.SupervisorID);
        ViewBag.CurriculumID = new SelectList(curricula,   "CurriculumID", "CurriculumName", selected?.CurriculumID);
    }
}

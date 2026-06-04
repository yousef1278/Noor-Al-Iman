using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCenterApp.Data;
using QuranCenterApp.Models;

namespace QuranCenterApp.Controllers;

public class GiftsController : Controller
{
    private readonly AppDbContext _db;

    public GiftsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        return View(await _db.Gifts.ToListAsync());
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Gift gift)
    {
        if (ModelState.IsValid)
        {
            _db.Gifts.Add(gift);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Gift added!";
            return RedirectToAction(nameof(Index));
        }
        return View(gift);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var g = await _db.Gifts.FindAsync(id);
        if (g == null) return NotFound();
        return View(g);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Gift gift)
    {
        if (id != gift.GiftID) return BadRequest();
        if (ModelState.IsValid)
        {
            _db.Update(gift);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Gift updated!";
            return RedirectToAction(nameof(Index));
        }
        return View(gift);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var g = await _db.Gifts.FindAsync(id);
        if (g == null) return NotFound();
        return View(g);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var g = await _db.Gifts.FindAsync(id);
        if (g != null) { _db.Gifts.Remove(g); await _db.SaveChangesAsync(); }
        TempData["Success"] = "Gift deleted!";
        return RedirectToAction(nameof(Index));
    }

    // Gift Distribution
    public async Task<IActionResult> Distribute()
    {
        await PopulateDistributionDropdowns();
        return View(new GiftDistribution { DateReceived = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Distribute(GiftDistribution dist)
    {
        ModelState.Remove("Gift");
        ModelState.Remove("Student");

        if (ModelState.IsValid)
        {
            try
            {
                var gift = await _db.Gifts.FindAsync(dist.GiftID);
                if (gift == null || gift.Quantity <= 0)
                {
                    ModelState.AddModelError("", "الهدية غير متوفرة أو نفذت الكمية!");
                    await PopulateDistributionDropdowns();
                    return View(dist);
                }
                // Trigger handles quantity decrement automatically
                _db.GiftDistributions.Add(dist);
                await _db.SaveChangesAsync();
                TempData["Success"] = "تم توزيع الهدية بنجاح!";
                return RedirectToAction(nameof(Distributions));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "خطأ: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
        await PopulateDistributionDropdowns();
        return View(dist);
    }

    public async Task<IActionResult> Distributions()
    {
        var list = await _db.GiftDistributions
            .Include(gd => gd.Gift)
            .Include(gd => gd.Student).ThenInclude(s => s.Person)
            .OrderBy(gd => gd.DateReceived).ThenBy(gd => gd.DistributionID)
            .ToListAsync();
        return View(list);
    }

    // Students with no gifts (report query)
    public async Task<IActionResult> StudentsWithNoGifts()
    {
        var studentsWithGifts = _db.GiftDistributions.Select(gd => gd.StudentID).Distinct();
        var noGiftStudents = await _db.Students
            .Include(s => s.Person)
            .Where(s => !studentsWithGifts.Contains(s.StudentID))
            .ToListAsync();
        return View(noGiftStudents);
    }

    private async Task PopulateDistributionDropdowns()
    {
        var students = await _db.Students
            .Include(s => s.Person)
            .Select(s => new { s.StudentID, Name = s.Person.FirstName + " " + s.Person.LastName })
            .ToListAsync();

        ViewBag.StudentID = new SelectList(students, "StudentID", "Name");

        var gifts = await _db.Gifts
            .Where(g => g.Quantity > 0)
            .Select(g => new { g.GiftID, Display = g.GiftName + " (" + g.GiftType + " - متبقي: " + g.Quantity + ")" })
            .ToListAsync();
        ViewBag.GiftID = new SelectList(gifts, "GiftID", "Display");
    }
}

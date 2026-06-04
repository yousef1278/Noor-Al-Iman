using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCenterApp.Data;
using QuranCenterApp.Models;

namespace QuranCenterApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.StudentCount   = await _db.Students.CountAsync();
        ViewBag.TeacherCount   = await _db.Teachers.CountAsync();
        ViewBag.ClassroomCount = await _db.Classrooms.CountAsync();
        ViewBag.GiftCount      = await _db.Gifts.SumAsync(g => (int?)g.Quantity) ?? 0;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

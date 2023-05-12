using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using YoutubeCategories.Repository;
using youtubecategoriesweb.Models;

namespace youtubecategoriesweb.Controllers
{
    public class HomeController : Controller
    {
        private ytvideoContext _context;

        public HomeController(ytvideoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {
            return View(_context.Regions.AsNoTracking().ToList());
        }

        public async Task<IActionResult> getcategories(long id)
        {
            return View(_context.Categories.AsNoTracking().Where(c=>c.RegionId == id).ToList());
        }

        public async Task<IActionResult> getvideos(long id)
        {
            return View(_context.Videos.AsNoTracking().Where(c => c.CategoryId == id).ToList());
        }

        public IActionResult Design()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CaloriesTracker.Services;

namespace CaloriesTracker.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly FileService _fileService;

        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var files = await _fileService.GetUserFilesAsync(userId);
            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "No file selected");
                return View("Index");
            }

            await _fileService.SaveFileAsync(file, userId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var file = await _fileService.GetFileAsync(id, userId);
            if (file == null) return NotFound();

            return File(file.Content, file.ContentType, file.FileName);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _fileService.DeleteFileAsync(id, userId);
            if (!success) return NotFound();

            return RedirectToAction("Index");
        }
    }
}

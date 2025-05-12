using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaloriesTracker.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortBy = "Name")
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var products = await _productService.GetUserProductsAsync(userId);

            products = sortBy switch
            {
                "Calories" => products.OrderByDescending(p => p.CaloriesPer100g).ToList(),
                "Protein" => products.OrderByDescending(p => p.ProteinPer100g).ToList(),
                _ => products.OrderBy(p => p.Name).ToList(),
            };

            ViewBag.SortBy = sortBy;
            return View(products);
        }


        [HttpPost]
        public async Task<IActionResult> Add(Product product)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var culture = System.Globalization.CultureInfo.InvariantCulture;

            try
            {
                product.CaloriesPer100g = Convert.ToDecimal(Request.Form["CaloriesPer100g"].ToString().Replace(',', '.'), culture);
                product.ProteinPer100g = Convert.ToDecimal(Request.Form["ProteinPer100g"].ToString().Replace(',', '.'), culture);
                product.FatPer100g = Convert.ToDecimal(Request.Form["FatPer100g"].ToString().Replace(',', '.'), culture);
                product.CarbsPer100g = Convert.ToDecimal(Request.Form["CarbsPer100g"].ToString().Replace(',', '.'), culture);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid number format. Use dot or comma as decimal separator.");
                return View("Index", await _productService.GetUserProductsAsync(userId));
            }

            product.UserId = userId;
            await _productService.AddProductAsync(product);
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var product = await _productService.GetProductByIdAsync(id, userId);
            if (product == null) return NotFound();

            return View(product);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _productService.DeleteProductAsync(id, userId);
            if (!success) return NotFound();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var product = await _productService.GetProductByIdAsync(id, userId);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var culture = System.Globalization.CultureInfo.InvariantCulture;

            try
            {
                product.CaloriesPer100g = Convert.ToDecimal(Request.Form["CaloriesPer100g"].ToString().Replace(',', '.'), culture);
                product.ProteinPer100g = Convert.ToDecimal(Request.Form["ProteinPer100g"].ToString().Replace(',', '.'), culture);
                product.FatPer100g = Convert.ToDecimal(Request.Form["FatPer100g"].ToString().Replace(',', '.'), culture);
                product.CarbsPer100g = Convert.ToDecimal(Request.Form["CarbsPer100g"].ToString().Replace(',', '.'), culture);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid number format. Use dot or comma as decimal separator.");
                return View(product);
            }

            if (!ModelState.IsValid) return View(product);

            product.UserId = userId;
            var success = await _productService.UpdateProductAsync(product, userId);

            if (!success) return NotFound();

            return RedirectToAction("Index");
        }


    }
}

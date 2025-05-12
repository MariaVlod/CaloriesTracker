using CaloriesTracker.Data;
using CaloriesTracker.Models;
using Microsoft.EntityFrameworkCore; 
using System.Diagnostics.CodeAnalysis;

namespace CaloriesTracker.Services
{
    public class ProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(int productId, string userId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);

            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Product>> GetUserProductsAsync(string userId)
        {
            return await _context.Products
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        [return: MaybeNull]
        public async Task<Product> GetProductByIdAsync(int productId, string userId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
        }

        public async Task<bool> UpdateProductAsync(Product updatedProduct, string userId)
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == updatedProduct.Id && p.UserId == userId);

            if (existingProduct == null) return false;

            existingProduct.Name = updatedProduct.Name;
            existingProduct.CaloriesPer100g = updatedProduct.CaloriesPer100g;
            existingProduct.ProteinPer100g = updatedProduct.ProteinPer100g;
            existingProduct.FatPer100g = updatedProduct.FatPer100g;
            existingProduct.CarbsPer100g = updatedProduct.CarbsPer100g;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

using CaloriesTracker.Data;
using CaloriesTracker.Models;
using Microsoft.EntityFrameworkCore; // Добавьте эту строку в начало файла
using System.Diagnostics.CodeAnalysis;

namespace CaloriesTracker.Services
{
    public class FileService
    {
        private readonly AppDbContext _context;

        public FileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FileRecord> SaveFileAsync(IFormFile file, string userId)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var fileRecord = new FileRecord
            {
                FileName = file.FileName,
                Content = memoryStream.ToArray(),
                ContentType = file.ContentType,
                UserId = userId
            };

            _context.FileRecords.Add(fileRecord);
            await _context.SaveChangesAsync();

            return fileRecord;
        }

        [return: MaybeNull]
        public async Task<FileRecord> GetFileAsync(int fileId, string userId)
        {
            return await _context.FileRecords
                .FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);
        }

        public async Task<List<FileRecord>> GetUserFilesAsync(string userId)
        {
            return await _context.FileRecords
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();
        }

        public async Task<bool> DeleteFileAsync(int fileId, string userId)
        {
            var file = await _context.FileRecords
                .FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);

            if (file == null) return false;

            _context.FileRecords.Remove(file);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

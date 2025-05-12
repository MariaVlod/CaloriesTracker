using System.ComponentModel.DataAnnotations.Schema;

namespace CaloriesTracker.Models
{
    public class FoodEntry
    {
        public int Id { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }

        [Column(TypeName = "decimal(8, 2)")]
        public decimal Calories { get; set; }
        public decimal Proteins { get; set; }
        public decimal Fats { get; set; }
        public decimal Carbohydrates { get; set; }
    }
}
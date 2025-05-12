namespace CaloriesTracker.Services
{
    public class ReportService
    {
        private readonly CalorieService _calorieService;

        public ReportService(CalorieService calorieService)
        {
            _calorieService = calorieService;
        }

        public async Task<string> GenerateDailyReportHtmlAsync(string userId, DateTime date)
        {
            var summary = await _calorieService.GetDailySummaryAsync(userId, date);

            var html = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Daily Calorie Report - {date:yyyy-MM-dd}</title>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .report-header {{ text-align: center; margin-bottom: 20px; }}
                .summary {{ margin-bottom: 20px; }}
                table {{ width: 100%; border-collapse: collapse; }}
                th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                th {{ background-color: #f2f2f2; }}
                .progress-container {{ width: 100%; background-color: #f1f1f1; }}
                .progress-bar {{ height: 30px; background-color: #4CAF50; text-align: center; line-height: 30px; color: white; }}
            </style>
        </head>
        <body>
            <div class='report-header'>
                <h1>Daily Calorie Report</h1>
                <h2>{date:yyyy-MM-dd}</h2>
            </div>
            
            <div class='summary'>
                <h3>Summary</h3>
                <p>Total Calories: {summary.TotalCalories:F0} / {summary.DailyGoal:F0}</p>
                <div class='progress-container'>
                    <div class='progress-bar' style='width: {Math.Min(100, (double)(summary.TotalCalories / summary.DailyGoal * 100))}%'>
                        {Math.Round(summary.TotalCalories / summary.DailyGoal * 100, 1)}%
                    </div>
                </div>
                <p>Protein: {summary.TotalProtein:F1}g</p>
                <p>Fat: {summary.TotalFat:F1}g</p>
                <p>Carbs: {summary.TotalCarbs:F1}g</p>
            </div>
            
            <h3>Consumed Products</h3>
            <table>
                <tr>
                    <th>Product</th>
                    <th>Quantity (g)</th>
                    <th>Calories</th>
                    <th>Protein</th>
                    <th>Fat</th>
                    <th>Carbs</th>
                </tr>";

            foreach (var intake in summary.Intakes)
            {
                html += $@"
                <tr>
                    <td>{intake.Product.Name}</td>
                    <td>{intake.Quantity:F1}</td>
                    <td>{intake.Product.CaloriesPer100g * intake.Quantity / 100:F1}</td>
                    <td>{intake.Product.ProteinPer100g * intake.Quantity / 100:F1}</td>
                    <td>{intake.Product.FatPer100g * intake.Quantity / 100:F1}</td>
                    <td>{intake.Product.CarbsPer100g * intake.Quantity / 100:F1}</td>
                </tr>";
            }

            html += @"
            </table>
        </body>
        </html>";

            return html;
        }
    }
}

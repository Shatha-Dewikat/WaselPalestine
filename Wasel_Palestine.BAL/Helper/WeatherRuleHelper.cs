using System;

namespace Wasel_Palestine.BLL.Helper
{
    public static class WeatherRuleHelper
    {
        public static (string Title, string Description, int SeverityId)? MapToIncident(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return null;

            // تحويل النص لحروف صغيرة للمقارنة بشكل أدق وتجنب مشاكل الحروف الكبيرة والصغيرة
            var lowerCondition = condition.ToLower();

            return lowerCondition switch
            {
                // استخدام Contains للبحث عن الكلمات المفتاحية داخل النص القادم
                var c when c.Contains("clear") || c.Contains("cloudy")
                    => ("Test Weather", "هذا بلاغ تجريبي للتأكد من عمل النظام في الجو الصافي أو الغائم جزئياً", 1),

                var c when c.Contains("fog") || c.Contains("mist")
                    => ("Low Visibility", "تحذير: رؤية منخفضة بسبب الضباب في هذه المنطقة", 2),

                var c when c.Contains("rain") || c.Contains("drizzle")
                    => ("Slippery Roads", "تحذير: طرق منزلقة بسبب الأمطار", 2),

                var c when c.Contains("snow")
                    => ("Snow Hazard", "خطر: تراكم ثلوج قد يؤدي لإغلاق طرق", 3),

                var c when c.Contains("thunderstorm")
                    => ("Severe Weather", "خطر: عواصف رعدية وظروف جوية قاسية", 3),

                _ => null
            };
        }
    }
}
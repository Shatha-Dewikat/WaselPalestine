using System;

namespace Wasel_Palestine.BLL.Helper
{
    public static class WeatherRuleHelper
    {
        public static (string Title, string Description, int SeverityId)? MapToIncident(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return null;

            var lowerCondition = condition.ToLower();

            return lowerCondition switch
            {
                var c when c.Contains("clear") || c.Contains("cloudy")
                    => ("Test Weather", "هذا بلاغ تجريبي للتأكد من عمل النظام في الجو الصافي أو الغائم جزئياً", 5),

                var c when c.Contains("fog") || c.Contains("mist")
                    => ("Low Visibility", "تحذير: رؤية منخفضة بسبب الضباب في هذه المنطقة", 6),

                var c when c.Contains("rain") || c.Contains("drizzle")
                    => ("Slippery Roads", "تحذير: طرق منزلقة بسبب الأمطار", 6),

                var c when c.Contains("snow")
                    => ("Snow Hazard", "خطر: تراكم ثلوج قد يؤدي لإغلاق طرق", 7),

                var c when c.Contains("thunderstorm")
                    => ("Severe Weather", "خطر: عواصف رعدية وظروف جوية قاسية", 7  ),

                _ => null
            };
        }
    }
}
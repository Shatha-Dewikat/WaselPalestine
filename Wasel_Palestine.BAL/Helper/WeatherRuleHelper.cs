using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.BAL.Helper
{
    public static class WeatherRuleHelper
    {
        public static (string Title, string Description, int SeverityId)? MapToIncident(string condition)
        {
            return condition switch
            {
                "Clear Sky" => ("Test Weather", "هذا بلاغ تجريبي للتأكد من عمل النظام في الجو الصافي", 1),
                "Fog" => ("Low Visibility", "تحذير: رؤية منخفضة بسبب الضباب في هذه المنطقة", 2),
                "Rain" => ("Slippery Roads", "تحذير: طرق منزلقة بسبب الأمطار", 2),
                "Snow Fall" => ("Snow Hazard", "خطر: تراكم ثلوج قد يؤدي لإغلاق طرق", 3),
                "Thunderstorm" => ("Severe Weather", "خطر: عواصف رعدية وظروف جوية قاسية", 3),
                _ => null
            };
        }
    }
}
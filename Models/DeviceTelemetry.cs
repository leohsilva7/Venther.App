using System;
using System.Collections.Generic;
using System.Text;

namespace Venther.App.Models
{
    public enum TrendPeriod { OneHour, TwelveHours, TwentyFourHours }

    public record MetricReading(double Value, string StatusText, bool IsAlert);

    public record DeviceTelemetry(
        MetricReading Supply,          // Temp. insuflamento (°C)
        MetricReading Return,          // Temp. retorno (°C)
        MetricReading GasPressure,     // Pressão linha de gás (bar)
        MetricReading EvaporatorDeltaP,// ΔP evaporador (Pa)
        int OperationalPercent);
}

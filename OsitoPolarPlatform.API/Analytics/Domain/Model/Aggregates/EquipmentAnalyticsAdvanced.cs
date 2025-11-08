using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Aggregates;

/// <summary>
/// Advanced analytics calculations for equipment
/// Partial class containing health, anomaly, cost, and maintenance forecast logic
/// </summary>
public partial class EquipmentAnalytics
{
    /// <summary>
    /// Calculate equipment health metrics based on temperature readings
    /// </summary>
    public static EquipmentHealth CalculateHealth(List<TemperatureReading> readings)
    {
        if (readings == null || readings.Count < 5)
        {
            return new EquipmentHealth
            {
                HealthScore = 0,
                IsStable = false,
                ReadingsAnalyzed = readings?.Count ?? 0,
                Trend = new TemperatureTrend { Direction = "unknown" }
            };
        }

        var temperatures = readings.Select(r => r.Temperature).ToList();

        // Calculate statistical measures
        var mean = temperatures.Average();
        var variance = temperatures.Select(t => Math.Pow((double)(t - mean), 2)).Average();
        var stdDev = (decimal)Math.Sqrt(variance);
        var min = temperatures.Min();
        var max = temperatures.Max();
        var range = max - min;

        // Calculate health score (0-100)
        // Lower variance = higher score
        var healthScore = Math.Max(0, 100 - (stdDev * 20));

        // Determine stability (stable if std dev < 1.0°C)
        var isStable = stdDev < 1.0m;

        // Calculate trend
        var trend = CalculateTrend(readings);

        return new EquipmentHealth
        {
            HealthScore = Math.Round(healthScore, 0),
            Mean = Math.Round(mean, 2),
            StandardDeviation = Math.Round(stdDev, 2),
            Variance = Math.Round((decimal)variance, 2),
            Range = Math.Round(range, 2),
            MinTemperature = Math.Round(min, 2),
            MaxTemperature = Math.Round(max, 2),
            IsStable = isStable,
            Trend = trend,
            ReadingsAnalyzed = readings.Count
        };
    }

    /// <summary>
    /// Calculate temperature trend using simple linear regression
    /// </summary>
    private static TemperatureTrend CalculateTrend(List<TemperatureReading> readings)
    {
        if (readings.Count < 3)
        {
            return new TemperatureTrend { Direction = "unknown" };
        }

        var sortedReadings = readings.OrderBy(r => r.Timestamp).ToList();
        var n = sortedReadings.Count;

        // Simple linear regression: y = mx + b
        decimal sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

        for (int i = 0; i < n; i++)
        {
            sumX += i;
            sumY += sortedReadings[i].Temperature;
            sumXY += i * sortedReadings[i].Temperature;
            sumX2 += i * i;
        }

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);

        // Determine direction
        string direction = "stable";
        if (slope > 0.05m) direction = "rising";
        else if (slope < -0.05m) direction = "falling";

        // Project change in 24 hours (assuming hourly readings)
        var projectedChange = slope * 24;

        return new TemperatureTrend
        {
            Direction = direction,
            Slope = Math.Round(slope, 4),
            ProjectedChange24h = Math.Round(projectedChange, 2)
        };
    }

    /// <summary>
    /// Detect temperature anomalies (door open, compressor failure, etc.)
    /// </summary>
    public static AnomalyDetection DetectAnomalies(List<TemperatureReading> readings, decimal optimalMin, decimal optimalMax)
    {
        if (readings == null || readings.Count < 3)
        {
            return new AnomalyDetection
            {
                HasAnomaly = false,
                Type = AnomalyType.Normal,
                Severity = AnomalySeverity.Normal,
                Message = "Insufficient data for anomaly detection",
                DetectedAt = DateTimeOffset.UtcNow,
                Confidence = 0
            };
        }

        var sortedReadings = readings.OrderBy(r => r.Timestamp).ToList();
        var recentReadings = sortedReadings.TakeLast(5).ToList();

        // Calculate temperature change rates (°C per minute)
        var changeRates = new List<decimal>();
        for (int i = 1; i < recentReadings.Count; i++)
        {
            var prev = recentReadings[i - 1];
            var curr = recentReadings[i];

            var tempDiff = curr.Temperature - prev.Temperature;
            var timeDiff = (decimal)(curr.Timestamp - prev.Timestamp).TotalMinutes;

            if (timeDiff > 0)
            {
                var changeRate = tempDiff / timeDiff;
                changeRates.Add(changeRate);
            }
        }

        if (!changeRates.Any())
        {
            return new AnomalyDetection
            {
                HasAnomaly = false,
                Type = AnomalyType.Normal,
                DetectedAt = DateTimeOffset.UtcNow
            };
        }

        var avgChangeRate = changeRates.Average();
        var lastReading = recentReadings.Last();
        var duration = (decimal)(lastReading.Timestamp - sortedReadings.First().Timestamp).TotalMinutes;

        // DOOR OPEN: Rapid temperature increase (>0.5°C per minute)
        if (avgChangeRate > 0.5m)
        {
            return new AnomalyDetection
            {
                HasAnomaly = true,
                Type = AnomalyType.DoorOpen,
                Severity = AnomalySeverity.Warning,
                Message = "🚪 Door likely open - temperature rising rapidly",
                Recommendation = "Check if refrigerator door is closed properly",
                ChangeRate = Math.Round(avgChangeRate, 2),
                DurationMinutes = Math.Round(duration, 1),
                ShouldAlert = false, // Don't panic for brief door opens
                DetectedAt = DateTimeOffset.UtcNow,
                Confidence = 80
            };
        }

        // RAPID COOLING: Unexpected temperature drop
        if (avgChangeRate < -0.5m)
        {
            return new AnomalyDetection
            {
                HasAnomaly = true,
                Type = AnomalyType.RapidCooling,
                Severity = AnomalySeverity.Warning,
                Message = "❄️ Rapid cooling detected",
                Recommendation = "Check thermostat settings and door seal",
                ChangeRate = Math.Round(avgChangeRate, 2),
                DurationMinutes = Math.Round(duration, 1),
                ShouldAlert = false,
                DetectedAt = DateTimeOffset.UtcNow,
                Confidence = 75
            };
        }

        // COMPRESSOR FAILURE: Slow steady rise over extended period
        if (avgChangeRate > 0.1m && avgChangeRate < 0.5m && duration > 120) // Rising for > 2 hours
        {
            return new AnomalyDetection
            {
                HasAnomaly = true,
                Type = AnomalyType.CompressorFailure,
                Severity = AnomalySeverity.Critical,
                Message = $"⚠️ Possible compressor failure - temperature rising slowly for {Math.Round(duration / 60, 1)} hours",
                Recommendation = "URGENT: Call technician immediately to avoid food spoilage",
                ChangeRate = Math.Round(avgChangeRate, 2),
                DurationMinutes = Math.Round(duration, 1),
                ShouldAlert = true, // This is a real emergency
                DetectedAt = DateTimeOffset.UtcNow,
                Confidence = 85
            };
        }

        // NORMAL
        return new AnomalyDetection
        {
            HasAnomaly = false,
            Type = AnomalyType.Normal,
            Severity = AnomalySeverity.Normal,
            Message = "✅ Temperature stable - no anomalies detected",
            Recommendation = "Equipment operating normally",
            ChangeRate = Math.Round(avgChangeRate, 2),
            DurationMinutes = Math.Round(duration, 1),
            ShouldAlert = false,
            DetectedAt = DateTimeOffset.UtcNow,
            Confidence = 90
        };
    }

    /// <summary>
    /// Calculate cost analysis based on energy readings
    /// </summary>
    public static CostAnalysis CalculateCosts(
        List<EnergyReading> currentMonthReadings,
        List<EnergyReading> previousMonthReadings,
        decimal electricityRate = 0.12m) // Default: $0.12 per kWh
    {
        var currentKwh = currentMonthReadings?.Sum(r => r.Consumption) ?? 0;
        var previousKwh = previousMonthReadings?.Sum(r => r.Consumption) ?? 0;

        var currentCost = currentKwh * electricityRate;
        var previousCost = previousKwh * electricityRate;
        var difference = currentCost - previousCost;
        var percentChange = previousCost > 0 ? (difference / previousCost) * 100 : 0;

        var daysAnalyzed = currentMonthReadings?.Count > 0
            ? (currentMonthReadings.Max(r => r.Timestamp) - currentMonthReadings.Min(r => r.Timestamp)).Days + 1
            : 0;

        var dailyAverage = daysAnalyzed > 0 ? currentCost / daysAnalyzed : 0;
        var projectedAnnual = dailyAverage * 365;

        // Determine trend
        string trend = "stable";
        if (Math.Abs(percentChange) > 5)
        {
            trend = percentChange > 0 ? "increasing" : "decreasing";
        }

        // Generate recommendations
        var recommendations = new List<string>();
        if (percentChange > 20)
        {
            recommendations.Add("Energy consumption increased significantly - schedule maintenance check");
        }
        if (percentChange > 10)
        {
            recommendations.Add("Clean condenser coils to improve efficiency");
        }

        return new CostAnalysis
        {
            CurrentMonthCost = Math.Round(currentCost, 2),
            PreviousMonthCost = Math.Round(previousCost, 2),
            Difference = Math.Round(difference, 2),
            PercentChange = Math.Round(percentChange, 1),
            Trend = trend,
            ProjectedAnnualCost = Math.Round(projectedAnnual, 2),
            DailyAverageCost = Math.Round(dailyAverage, 2),
            TotalKwh = Math.Round(currentKwh, 2),
            ElectricityRate = electricityRate,
            DaysAnalyzed = daysAnalyzed,
            PotentialSavings = percentChange > 15 ? Math.Round(currentCost * 0.15m, 2) : null,
            Recommendations = recommendations
        };
    }

    /// <summary>
    /// Forecast maintenance needs using simple rule-based calculations
    /// No ML - just variance and trend analysis
    /// </summary>
    public static MaintenanceForecast ForecastMaintenance(
        List<TemperatureReading> readings,
        DateTimeOffset? installationDate,
        decimal optimalMin,
        decimal optimalMax)
    {
        if (readings == null || readings.Count < 10)
        {
            return new MaintenanceForecast
            {
                RiskScore = 0,
                RiskLevel = ValueObjects.RiskLevel.Low,
                Confidence = "low",
                Recommendation = "Insufficient data for maintenance forecast"
            };
        }

        var health = CalculateHealth(readings);
        var indicators = new RiskIndicators();
        var reasons = new List<string>();
        var riskScore = 0;

        // 1. VARIANCE SCORE (0-40 points)
        // High variance = equipment struggling
        if (health.StandardDeviation > 3.0m)
        {
            indicators = indicators with { VarianceScore = 40 };
            riskScore += 40;
            reasons.Add($"High temperature variance ({health.StandardDeviation}°C) indicates equipment stress");
        }
        else if (health.StandardDeviation > 2.0m)
        {
            indicators = indicators with { VarianceScore = 25 };
            riskScore += 25;
            reasons.Add($"Moderate temperature variance ({health.StandardDeviation}°C)");
        }
        else if (health.StandardDeviation > 1.0m)
        {
            indicators = indicators with { VarianceScore = 10 };
            riskScore += 10;
        }

        // 2. EFFICIENCY SCORE (0-30 points)
        // Based on temperature range relative to optimal
        var optimalRange = optimalMax - optimalMin;
        var actualRange = health.Range;
        var rangeRatio = optimalRange > 0 ? actualRange / optimalRange : 0;

        if (rangeRatio > 2.0m)
        {
            indicators = indicators with { EfficiencyScore = 30 };
            riskScore += 30;
            reasons.Add("Temperature range exceeds optimal by 2x - efficiency declining");
        }
        else if (rangeRatio > 1.5m)
        {
            indicators = indicators with { EfficiencyScore = 20 };
            riskScore += 20;
            reasons.Add("Temperature range exceeds optimal");
        }

        // 3. AGE SCORE (0-20 points)
        if (installationDate.HasValue)
        {
            var ageInMonths = (DateTimeOffset.UtcNow - installationDate.Value).Days / 30;

            if (ageInMonths > 60) // > 5 years
            {
                indicators = indicators with { AgeScore = 20 };
                riskScore += 20;
                reasons.Add($"Equipment age ({ageInMonths / 12} years) - typical service life exceeded");
            }
            else if (ageInMonths > 36) // > 3 years
            {
                indicators = indicators with { AgeScore = 10 };
                riskScore += 10;
                reasons.Add($"Equipment age ({ageInMonths / 12} years) - routine maintenance due");
            }
        }

        // 4. TREND SCORE (0-10 points)
        if (health.Trend.Direction == "rising" && health.Trend.ProjectedChange24h > 1.0m)
        {
            indicators = indicators with { TrendScore = 10 };
            riskScore += 10;
            reasons.Add($"Rising temperature trend ({health.Trend.ProjectedChange24h}°C/24h projected)");
        }

        // Determine risk level and recommendation
        string riskLevel;
        string recommendation;
        int? daysToMaintenance;
        decimal? estimatedCost;

        if (riskScore >= 70)
        {
            riskLevel = ValueObjects.RiskLevel.Critical;
            recommendation = "URGENT: Schedule emergency maintenance within 3 days to prevent failure";
            daysToMaintenance = 3;
            estimatedCost = 200m; // Emergency service
        }
        else if (riskScore >= 50)
        {
            riskLevel = ValueObjects.RiskLevel.High;
            recommendation = "Schedule maintenance within 1-2 weeks to prevent issues";
            daysToMaintenance = 14;
            estimatedCost = 150m;
        }
        else if (riskScore >= 30)
        {
            riskLevel = ValueObjects.RiskLevel.Moderate;
            recommendation = "Schedule routine maintenance within 30 days";
            daysToMaintenance = 30;
            estimatedCost = 100m;
        }
        else
        {
            riskLevel = ValueObjects.RiskLevel.Low;
            recommendation = "Equipment operating well - no immediate maintenance needed";
            daysToMaintenance = null;
            estimatedCost = null;
        }

        return new MaintenanceForecast
        {
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            EstimatedDaysToMaintenance = daysToMaintenance,
            Confidence = readings.Count > 100 ? "high" : readings.Count > 50 ? "medium" : "low",
            Indicators = indicators,
            Recommendation = recommendation,
            EstimatedCost = estimatedCost,
            Reasons = reasons
        };
    }
}

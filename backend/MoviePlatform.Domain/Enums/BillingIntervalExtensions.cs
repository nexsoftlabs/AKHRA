namespace MoviePlatform.Domain.Enums;

public static class BillingIntervalExtensions
{
    public static int PeriodMonths(BillingInterval interval) =>
        interval switch
        {
            BillingInterval.Monthly => 1,
            BillingInterval.ThreeMonths => 3,
            BillingInterval.SixMonths => 6,
            BillingInterval.Annual => 12,
            _ => 1,
        };

    public static (string RazorpayPeriod, int RazorpayInterval) ToRazorpay(BillingInterval interval) =>
        interval switch
        {
            BillingInterval.Annual => ("yearly", 1),
            BillingInterval.ThreeMonths => ("monthly", 3),
            BillingInterval.SixMonths => ("monthly", 6),
            _ => ("monthly", 1),
        };

    public static string DisplayLabel(BillingInterval interval) =>
        interval switch
        {
            BillingInterval.Monthly => "month",
            BillingInterval.ThreeMonths => "3 months",
            BillingInterval.SixMonths => "6 months",
            BillingInterval.Annual => "year",
            _ => "period",
        };
}

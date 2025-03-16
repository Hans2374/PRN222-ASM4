namespace PaymentCVSTS.BlazorApp.Enums
{
    public enum PaymentStatusEnum : byte
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4
    }
}
namespace CarDealership.Services;

public enum OtpErrorCode
{
    None = 0,
    NotFound = 1,
    Locked = 2,
    Invalid = 3
}

public class OtpVerificationResult
{
    public bool Success { get; set; }
    public OtpErrorCode ErrorCode { get; set; } = OtpErrorCode.None;
    public string Message { get; set; } = string.Empty;
}



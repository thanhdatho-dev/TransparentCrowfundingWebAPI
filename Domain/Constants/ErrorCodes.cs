namespace Domain.Constants
{
    public static class ErrorCodes
    {
        // Auth
        public const string InvalidWallet = "INVALID_WALLET";
        public const string EmailAlreadyUsed = "EMAIL_ALREADY_USED";
        public const string EmailNotConfirmed = "EMAIL_NOT_CONFIRMED";
        public const string InvalidOTP = "INVALID_OTP";
        public const string OTPMismatch = "OTP_MISMATCH";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";

        // Entity
        public const string NotFound = "NOT_FOUND";
        public const string AlreadyExists = "ALREADY_EXISTS";

        // Validation
        public const string ValidationError = "VALIDATION_ERROR";
    }
}

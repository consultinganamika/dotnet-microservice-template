namespace Employee.API.Extensions
{
    public static class MaskingExtensions
    {
        public static IServiceCollection AddMaskingServices(
            this IServiceCollection services)
        {
            services.AddScoped<IMaskingService, MaskingService>();
            return services;
        }
    }

    public interface IMaskingService
    {
        string MaskEmail(string email);
        string MaskPhoneNumber(string phoneNumber);
        string MaskSocialSecurityNumber(string ssn);
        string MaskCreditCard(string cardNumber);
    }

    public class MaskingService : IMaskingService
    {
        public string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            var localPart = parts[0];
            var domain = parts[1];

            var maskedLocalPart = localPart.Length > 2
                ? $"{localPart.Substring(0, 1)}***{localPart.Substring(localPart.Length - 1)}"
                : "***";

            return $"{maskedLocalPart}@{domain}";
        }

        public string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
                return phoneNumber;

            var last4 = phoneNumber.Substring(phoneNumber.Length - 4);
            return $"***-***-{last4}";
        }

        public string MaskSocialSecurityNumber(string ssn)
        {
            if (string.IsNullOrEmpty(ssn) || ssn.Length < 4)
                return ssn;

            return $"***-**-{ssn.Substring(ssn.Length - 4)}";
        }

        public string MaskCreditCard(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            var last4 = cardNumber.Substring(cardNumber.Length - 4);
            return $"****-****-****-{last4}";
        }
    }
}

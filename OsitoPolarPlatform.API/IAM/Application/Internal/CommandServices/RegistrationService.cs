using OsitoPolarPlatform.API.IAM.Application.Internal.OutboundServices;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Domain.Repositories;
using OsitoPolarPlatform.API.IAM.Domain.Services;
using OsitoPolarPlatform.API.IAM.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.Notifications.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;

namespace OsitoPolarPlatform.API.IAM.Application.Internal.CommandServices;

/// <summary>
/// Service for handling complete user registration with payment and profile creation
/// </summary>
public class RegistrationService : IRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IRenterProviderRepository _providerRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IHashingService _hashingService;
    private readonly IEmailCommandService _emailCommandService;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrationService(
        IUserRepository userRepository,
        IOwnerRepository ownerRepository,
        IRenterProviderRepository providerRepository,
        ISubscriptionRepository subscriptionRepository,
        IPaymentProvider paymentProvider,
        IHashingService hashingService,
        IEmailCommandService emailCommandService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _ownerRepository = ownerRepository;
        _providerRepository = providerRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentProvider = paymentProvider;
        _hashingService = hashingService;
        _emailCommandService = emailCommandService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterWithPaymentResponse> RegisterWithPaymentAsync(RegisterWithPaymentResource request)
    {
        try
        {
            Console.WriteLine($"[Registration] Starting registration for {request.Username} as {request.UserType}");

            // 1. Validate user type
            if (request.UserType != "Owner" && request.UserType != "Provider")
            {
                return new RegisterWithPaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid user type. Must be 'Owner' or 'Provider'"
                };
            }

            // 2. Validate plan ID matches user type
            var subscription = await _subscriptionRepository.FindByIdAsync(request.PlanId);
            if (subscription == null)
            {
                return new RegisterWithPaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Invalid plan ID: {request.PlanId}"
                };
            }

            // Validate plan type matches user type
            var isOwnerPlan = subscription.MaxEquipment.HasValue;
            var isProviderPlan = subscription.MaxClients.HasValue;

            if (request.UserType == "Owner" && !isOwnerPlan)
            {
                return new RegisterWithPaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Selected plan is not an Owner plan (plans 1-3)"
                };
            }

            if (request.UserType == "Provider" && !isProviderPlan)
            {
                return new RegisterWithPaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Selected plan is not a Provider plan (plans 4-6)"
                };
            }

            // 3. Check if username already exists
            if (_userRepository.ExistsByUsername(request.Username))
            {
                return new RegisterWithPaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Username {request.Username} is already taken"
                };
            }

            // 4. Check if email is already used
            var existingOwner = await _ownerRepository.FindByEmailAsync(request.Email);
            var existingProvider = await _providerRepository.FindByEmailAsync(request.Email);

            if (existingOwner != null || existingProvider != null)
            {
                return new RegisterWithPaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Email {request.Email} is already registered"
                };
            }

            // 5. Process payment with Stripe
            Console.WriteLine($"[Registration] Processing payment of {subscription.Price.Amount} {subscription.Price.Currency}");

            var paymentRequest = new PaymentRequest
            {
                Amount = subscription.Price.Amount,
                Currency = subscription.Price.Currency,
                Description = $"{subscription.PlanName} subscription for {request.Username}",
                CustomerEmail = request.Email,
                CustomerName = $"{request.FirstName} {request.LastName}",
                PaymentToken = request.PaymentToken,
                Metadata = new Dictionary<string, string>
                {
                    { "username", request.Username },
                    { "userType", request.UserType },
                    { "planId", request.PlanId.ToString() },
                    { "planName", subscription.PlanName }
                }
            };

            var paymentResult = await _paymentProvider.CreatePaymentAsync(paymentRequest);

            if (!paymentResult.Success)
            {
                Console.WriteLine($"[Registration] Payment failed: {paymentResult.ErrorMessage}");
                return new RegisterWithPaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Payment failed: {paymentResult.ErrorMessage}"
                };
            }

            Console.WriteLine($"[Registration] Payment succeeded: {paymentResult.TransactionId}");

            // 6. Generate random password
            var generatedPassword = GenerateSecurePassword();

            // 7. Create user account
            var hashedPassword = _hashingService.HashPassword(generatedPassword);
            var user = new User(request.Username, hashedPassword);
            await _userRepository.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            Console.WriteLine($"[Registration] User account created with ID: {user.Id}");

            // 8. Create Owner or Provider profile
            int profileId;

            if (request.UserType == "Owner")
            {
                var owner = new Owner(
                    user.Id,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Street,
                    request.Number,
                    request.City,
                    request.PostalCode,
                    request.Country,
                    request.PlanId,
                    subscription.MaxEquipment!.Value
                );

                await _ownerRepository.AddAsync(owner);
                await _unitOfWork.CompleteAsync();
                profileId = owner.Id;

                Console.WriteLine($"[Registration] Owner profile created with ID: {profileId}");
            }
            else // Provider
            {
                if (string.IsNullOrEmpty(request.CompanyName))
                {
                    return new RegisterWithPaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Company name is required for Provider registration"
                    };
                }

                var provider = new RenterProvider(
                    user.Id,
                    request.CompanyName,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Street,
                    request.Number,
                    request.City,
                    request.PostalCode,
                    request.Country,
                    request.PlanId,
                    subscription.MaxClients!.Value,
                    request.TaxId
                );

                await _providerRepository.AddAsync(provider);
                await _unitOfWork.CompleteAsync();
                profileId = provider.Id;

                Console.WriteLine($"[Registration] Provider profile created with ID: {profileId}");
            }

            // 9. Send welcome email with credentials
            try
            {
                var emailSubject = $"Welcome to OsitoPolar - Your Account Details";
                var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f4f4f4; padding: 20px; }}
        .credentials {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #3498db; }}
        .button {{ background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; display: inline-block; margin: 15px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🐻 Welcome to OsitoPolar!</h1>
        </div>
        <div class=""content"">
            <h2>Hello {request.FirstName}!</h2>
            <p>Thank you for registering as a <strong>{request.UserType}</strong> with OsitoPolar.</p>
            <p>Your subscription to <strong>{subscription.PlanName}</strong> has been activated.</p>

            <div class=""credentials"">
                <h3>Your Login Credentials:</h3>
                <p><strong>Username:</strong> {request.Username}</p>
                <p><strong>Password:</strong> {generatedPassword}</p>
                <p style=""color: #e74c3c; font-size: 14px;"">⚠️ Please change your password after your first login for security.</p>
            </div>

            <p><strong>Plan Details:</strong></p>
            <ul>
                <li>Plan: {subscription.PlanName}</li>
                <li>Price: ${subscription.Price.Amount}/month</li>
                {(request.UserType == "Owner" ? $"<li>Max Equipment: {subscription.MaxEquipment} units</li>" : $"<li>Max Clients: {(subscription.MaxClients == 0 ? "Unlimited" : subscription.MaxClients.ToString())}</li>")}
            </ul>

            <p style=""text-align: center;"">
                <a href=""http://localhost:3000/login"" class=""button"">Login Now</a>
            </p>

            <p>If you have any questions, please don't hesitate to contact our support team.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 OsitoPolar. All rights reserved.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";

                await _emailCommandService.SendRawEmailAsync(
                    request.Email,
                    $"{request.FirstName} {request.LastName}",
                    emailSubject,
                    emailBody
                );

                Console.WriteLine($"[Registration] Welcome email sent to {request.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Registration] Failed to send email: {ex.Message}");
                // Don't fail registration if email fails
            }

            // 10. Return success response
            return new RegisterWithPaymentResponse
            {
                Success = true,
                Message = "Registration completed successfully! Check your email for login credentials.",
                UserId = user.Id,
                Username = request.Username,
                GeneratedPassword = generatedPassword, // Include in response for testing/demo
                UserType = request.UserType,
                ProfileId = profileId,
                TransactionId = paymentResult.TransactionId
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Registration] Error: {ex.Message}");
            Console.WriteLine($"[Registration] Stack trace: {ex.StackTrace}");

            return new RegisterWithPaymentResponse
            {
                Success = false,
                ErrorMessage = $"Registration failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Generate a secure random password
    /// </summary>
    private string GenerateSecurePassword()
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string allChars = uppercase + lowercase + digits + special;

        var random = new Random();
        var password = new char[12];

        // Ensure at least one of each type
        password[0] = uppercase[random.Next(uppercase.Length)];
        password[1] = lowercase[random.Next(lowercase.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        // Fill the rest randomly
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        // Shuffle
        return new string(password.OrderBy(x => random.Next()).ToArray());
    }
}

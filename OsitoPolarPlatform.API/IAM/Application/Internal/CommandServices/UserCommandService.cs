using OsitoPolarPlatform.API.IAM.Application.Internal.OutboundServices;
using OsitoPolarPlatform.API.IAM.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.IAM.Domain.Model.Commands;
using OsitoPolarPlatform.API.IAM.Domain.Repositories;
using OsitoPolarPlatform.API.IAM.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.IAM.Application.Internal.CommandServices;

/**
 * <summary>
 *     The user command service
 * </summary>
 * <remarks>
 *     This class is used to handle user commands
 * </remarks>
 */
public class UserCommandService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork)
    : IUserCommandService
{
    /**
     * <summary>
     *     Handle sign in command
     * </summary>
     * <param name="command">The sign in command</param>
     * <returns>The authenticated user and the JWT token</returns>
     */
    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        var user = await userRepository.FindByUsernameAsync(command.Username);

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new Exception("Invalid username or password");

        var token = tokenService.GenerateToken(user);

        return (user, token);
    }

    /**
     * <summary>
     *     Handle sign-up command
     * </summary>
     * <param name="command">The sign-up command</param>
     * <returns>A confirmation message on successful creation.</returns>
     */
    public async Task Handle(SignUpCommand command)
    {
        try
        {
            Console.WriteLine($"[SignUp] Attempting to create user: {command.Username}");
            Console.WriteLine($"[SignUp] Password length: {command.Password?.Length ?? 0}");
        
            if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            {
                Console.WriteLine("[SignUp] ERROR: Username or password is empty");
                throw new ArgumentException("Username and password cannot be empty");
            }
            
            Console.WriteLine("[SignUp] Checking if username exists...");
            if (userRepository.ExistsByUsername(command.Username))
            {
                Console.WriteLine($"[SignUp] ERROR: Username {command.Username} already exists");
                throw new Exception($"Username {command.Username} is already taken");
            }

            Console.WriteLine("[SignUp] Hashing password...");
            var hashedPassword = hashingService.HashPassword(command.Password);
        
            Console.WriteLine("[SignUp] Creating user object...");
            var user = new User(command.Username, hashedPassword);
    
            Console.WriteLine("[SignUp] Adding user to repository...");
            await userRepository.AddAsync(user);
        
            Console.WriteLine("[SignUp] Completing transaction...");
            await unitOfWork.CompleteAsync();
        
            Console.WriteLine($"[SignUp] SUCCESS: User {command.Username} created successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SignUp] EXCEPTION: {e.GetType().Name}: {e.Message}");
            Console.WriteLine($"[SignUp] Stack Trace: {e.StackTrace}");
            throw new Exception($"An error occurred while creating user: {e.Message}");
        }
    }
}
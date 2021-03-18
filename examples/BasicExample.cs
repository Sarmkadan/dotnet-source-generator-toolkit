#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Examples;

/// Demonstrates basic usage of the toolkit for repository, mapper, and validator generation.
public sealed class BasicExample
{
    // Define domain entities with generation attributes
    [Repository]
    [Mapper]
    [Validator]
    /// <summary>
    /// Represents a user entity with properties for identification, email, first name, last name, creation date, and activity status.
    /// </summary>
    public sealed class User
    {
        /// <summary>
        /// Gets the unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the date and time when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets a value indicating whether the user is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    // DTO for API responses
    [Mapper]
    /// <summary>
    /// Represents a user data transfer object (DTO) with properties for identification and email.
    /// </summary>
    public sealed class UserDto
    {
        /// <summary>
        /// Gets the unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string LastName { get; set; } = string.Empty;
    }

    // Usage example
    public sealed class UserService
    {
        /// <summary>
        /// Retrieves a user by their unique identifier and returns the corresponding DTO.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to retrieve.</param>
        /// <param name="repository">The repository instance to use for data access.</param>
        /// <param name="mapper">The mapper instance to use for data transformation.</param>
        /// <param name="validator">The validator instance to use for data validation.</param>
        /// <returns>The user DTO if the user is found, otherwise null.</returns>
        public async Task<UserDto?> GetUserAsync(
            int userId,
            IUserRepository repository,
            IUserMapper mapper,
            IUserValidator validator)
        {
            // Fetch from repository (generated)
            var user = await repository.GetByIdAsync(userId);
            if (user is null)
                return null;

            // Validate before returning (generated)
            var validationResult = await validator.ValidateAsync(user);
            if (!validationResult.IsValid)
                throw new InvalidOperationException("User validation failed");

            // Map to DTO (generated)
            return mapper.MapToDto(user);
        }

        /// <summary>
        /// Creates a new user based on the provided DTO and returns the created user entity.
        /// </summary>
        /// <param name="dto">The user DTO to create a new user from.</param>
        /// <param name="repository">The repository instance to use for data access.</param>
        /// <param name="mapper">The mapper instance to use for data transformation.</param>
        /// <param name="validator">The validator instance to use for data validation.</param>
        /// <returns>The created user entity.</returns>
        public async Task<User> CreateUserAsync(
            UserDto dto,
            IUserRepository repository,
            IUserMapper mapper,
            IUserValidator validator)
        {
            // Map from DTO (generated)
            var user = mapper.MapFromDto(dto);

            // Validate before creating (generated)
            var validationResult = await validator.ValidateAsync(user);
            if (!validationResult.IsValid)
                throw new InvalidOperationException("User validation failed");

            // Create in repository (generated)
            return await repository.CreateAsync(user);
        }
    }
}

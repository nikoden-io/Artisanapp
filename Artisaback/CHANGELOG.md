# Changelog - Artisaback


## Table of Contents  

- [Unreleased](#unreleased)
- [Released](#released)
  - [v0.1.0 - 2025-04-02](#v010---2025-04-02)

## Unreleased

## Released

### v0.1.0 - 2025-04-02
>Authentication & Integration Testing

#### Added
- Authentication Endpoints
  - /api/auth/register: Allows user registration with role-based restrictions. Public registration now rejects "Admin" role.
  - /api/auth/login: Implements user login with password verification and JWT issuance.
  - /api/auth/refresh-token: Implements refresh token functionality, validating and updating tokens.
- Role-Based Authorization
  - Controller endpoints (e.g., /api/admin/users, /api/artisans/{id}/products, /api/delivery/orders) now secured using [Authorize(Roles = "...")].
- Admin Seeding
  - Automatic seeding of an admin account (via configuration settings) if no admin exists, ensuring secured access to administrative endpoints.
#### Changed
- MongoDbContext
  - Removed hard-coded connection string; now entirely configured via DI and external configuration.
- Program.cs
  - Updated to read MongoDB connection string and database name from configuration (appsettings.json or environment variables).
  - Configured JWT authentication and authorization with settings from configuration.
- CustomWebApplicationFactory
  - Implemented using Testcontainers for MongoDB:
  - Injects an in-memory configuration that overrides production values with the Testcontainers’ connection string and uses a dedicated test database ("TestDb").
  - Removes and re-registers the MongoDbContext to ensure tests point to the container instance.
  - Environment forced to "Test" in the factory to guarantee isolation.
#### Added Tests
- Unit Tests
  - AuthService unit tests verifying:
    - JWT token generation for registration.
    - Correct behavior on login with valid/invalid credentials.
    - Refresh token logic (with repository mocks).
- Integration Tests
  - AuthIntegrationTests covering:
    - Successful registration and token issuance.
    - Rejection of registration with "Admin" role.
    - Successful login and token retrieval.
    - Successful token refresh using a real refresh token read from the test database.
    - Duplicate email registration returns proper error.
    - Unauthenticated access to protected endpoints returns proper HTTP status.
  - RoleBasedAuthorizationTests verifying access rights for Admin, Artisan, Customer, and DeliveryPartner roles.
  - Middleware Tests
  - ExceptionMiddleware test ensuring UnauthorizedAccessException is properly handled.

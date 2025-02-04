# Artisaback 

## Authentication Process
### Overview
Artisaback implements a secure, token-based authentication system that supports role-based authorization. The system uses JSON Web Tokens (JWT) to manage user sessions. This document explains how the backend handles authentication and how the future client will interact with the system to maintain an active session without repeatedly asking for user credentials.

### Backend Authentication Flow
1. User Registration
   Endpoint: POST **/api/auth/register**  
   Payload Example:
   ```json
   {
       "Email": "user@example.com",
       "Password": "P@ssw0rd",
       "Role": "Admin" 
   }
   ```
   Other possible roles: **Artisan, Customer, DeliveryPartner**   
   Process:
   - The backend verifies the uniqueness of the email.
   - The password is hashed (using BCrypt) and stored securely.
   - A new user record is created with an initial refresh token and its expiry time.
   - A JWT is generated containing the user’s email and role.  
   Response:
   HTTP 201 Created along with a JSON payload:
   ```json
   { "Token": "generated_jwt_token" }
   ```
   User Login
   Endpoint: POST /api/auth/login
   Payload Example:
   ```json
   {
       "Email": "user@example.com",
       "Password": "P@ssw0rd"
   }
   ```
   Process:
   - The credentials are validated against the stored hash.
   - On success, a new JWT is generated and the refresh token is updated.  
   Response:
   HTTP 200 OK along with a JSON payload:
   ```json
   { "Token": "generated_jwt_token" }
   ```
3. Token Refresh
   Endpoint: POST /api/auth/refresh-token
   Payload Example:
   ```json
   {
   "RefreshToken": "existing_refresh_token"
   }
   ``` 
   Process:
   The backend validates the refresh token and checks its expiry.
   If valid, the refresh token is updated and a new JWT is generated.
   Response:
   HTTP 200 OK along with a JSON payload:
   ```json
   { "Token": "new_generated_jwt_token" }
   ```
4. Role-Based Authorization
   Implementation:
   - Endpoints are protected using role-based attributes (e.g., [Authorize(Roles = "Admin")]). This ensures that 
   only users with the proper roles can access specific endpoints.
   Example:
   An endpoint like /api/admin/users is accessible only by users with the Admin role.
   Unauthorized access attempts result in HTTP 401 (Unauthenticated) or 403 (Forbidden).
   Client-Side Authentication Process (Future Implementation)
1. User Login and Registration
   Process:
   - The client sends registration or login requests with the user’s credentials.
   - On success, the client receives a JWT token (and potentially a refresh token stored securely).
   - The JWT token is stored in a secure storage (e.g., HTTP-only cookies or secure local storage) on the client.
2. Accessing Protected Endpoints
   Process:
   - For each API request to a protected endpoint, the client attaches the JWT in the HTTP Authorization header:
   Authorization: Bearer generated_jwt_token
   - The backend verifies the token’s validity, role, issuer, and audience before allowing access.
3. Maintaining an Active Session
   Session Maintenance:
   JWT tokens typically have a short expiration time (e.g., 60 minutes) to minimize security risks.
   The client monitors token expiry and, before the token expires, silently sends a request to the refresh-token 
   endpoint using the stored refresh token. A successful refresh returns a new JWT (and updates the refresh token), 
   allowing the session to continue without prompting the user for their credentials.
   Client Responsibilities:
   - Securely store both the JWT and refresh token.
   - Implement logic to detect token expiry (e.g., using token payload expiry info).
   - Automatically trigger a token refresh request in the background when needed.
   - Handle error cases (e.g., if the refresh token is invalid or expired, prompt the user to log in again).
4. Security Best Practices
   On the Backend:
   - Use strong hashing (BCrypt) for passwords.
   - Sign JWTs with a secure, secret key.
   - Enforce issuer, audience, and expiration validations on tokens.
   - Handle exceptions (such as duplicate registrations) gracefully.
   On the Client:
   - Store tokens securely to prevent XSS or CSRF attacks.
   - Use HTTPS for all communications.
   - Keep the token refresh mechanism transparent to users to provide a seamless experience.
   Conclusion
   The Artisaback authentication system is designed to provide a secure and user-friendly experience:
   - Backend: 
     - Ensures robust authentication and authorization through JWTs, refresh tokens, and role-based access control.  
   - Client: 
     - Will maintain active sessions by automatically refreshing tokens, thus reducing the need for users to frequently
     re-enter credentials. This design not only secures access to endpoints but also supports a smooth user experience
     by keeping sessions active and secure.
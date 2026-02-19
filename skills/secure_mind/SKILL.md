---
name: Secure Mind (Security & Privacy)
description: Specialized skill for securing .NET + Next.js applications, focusing on authentication, authorization, and data privacy for sensitive health information.
---

# Secure Mind Skill: Privacy & Security

In tele-psychology, security isn't a feature; it's the foundation. This skill ensures your application protects sensitive patient data according to professional standards.

## 1. Authentication (JWT + Refresh Tokens)
Secure the communication between Next.js and .NET using structured JWT tokens.

```csharp
// .NET: JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
```

## 2. Authorization (RBAC)
Implement Role-Based Access Control to distinguish between Patients, Therapists, and Admins.

```csharp
[Authorize(Roles = "Therapist")]
[HttpGet("my-patients")]
public async Task<IActionResult> GetMyPatients() { ... }
```

## 3. Data Privacy (PII Protection)
- **Encryption**: Encrypt sensitive notes in the database using modern standards (AES-256).
- **Logging**: Never log Personally Identifiable Information (PII) like names or emails in application logs.
- **Audit Trails**: Track who accessed which patient record and when.

## 4. Frontend Security (Next.js)
- **HttpOnly Cookies**: Store session tokens in HttpOnly cookies to prevent XSS attacks.
- **Middleware Protection**: Use Next.js Middleware to protect private routes.
- **Input Sanitization**: Always validate and sanitize user input to prevent SQL Injection and XSS.

```typescript
// middleware.ts
export function middleware(request: NextRequest) {
  const token = request.cookies.get('session-token');
  if (!token && request.nextUrl.pathname.startsWith('/dashboard')) {
    return NextResponse.redirect(new URL('/login', request.url));
  }
}
```

## 5. Security Headers
Configure your app to use strict security headers to mitigate common web vulnerabilities.
- **CSP (Content Security Policy)**
- **X-Frame-Options: DENY**
- **Strict-Transport-Security**

## Best Practices
- **HTTPS Only**: Enforce HTTPS in all environments.
- **Minimal Data Disclosure**: Only send the minimum required data in API responses.
- **Dependency Scanning**: Regularly check for vulnerabilities in NuGet and NPM packages.
- **Rate Limiting**: Implement rate limiting on sensitive endpoints (Login, Reset Password).

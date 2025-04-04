# ProPulse Phase 1 Technical Specification

This technical specification covers the implementation details for Phase 1 of ProPulse, focusing on delivering the basic blogging platform with minimal social media sharing capabilities.

## Core Services Implementation

Below are the technical specifications for each microservice that will be implemented in Phase 1.

## 1. Identity Service

### 1.1 Data Model

#### ApplicationUser (extends IdentityUser)

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| Id | string | Primary key from ASP.NET Identity | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| UserName | string | Username for login purposes (from IdentityUser) | `johndoe` |
| Email | string | Email address for the user (from IdentityUser) | `john.doe@example.com` |
| EmailConfirmed | bool | Whether the email has been confirmed (from IdentityUser) | `true` |
| PhoneNumber | string | User's phone number (from IdentityUser) | `+1234567890` |
| PhoneNumberConfirmed | bool | Whether the phone number has been confirmed (from IdentityUser) | `false` |
| TwoFactorEnabled | bool | Whether 2FA is enabled (from IdentityUser) | `true` |
| LockoutEnd | DateTimeOffset? | When lockout ends (from IdentityUser) | `2025-04-10T10:15:30Z` |
| LockoutEnabled | bool | Whether lockout is enabled (from IdentityUser) | `true` |
| AccessFailedCount | int | Count of failed access attempts (from IdentityUser) | `0` |
| DisplayName | string | User's display name (extension) | `John Doe` |
| CreatedAt | DateTimeOffset | Date when account was created (extension, DB-managed) | `2025-04-03T10:15:30Z` |
| LastLoginAt | DateTimeOffset? | Date of last login (extension) | `2025-04-03T14:22:15Z` |
| IsActive | bool | Whether the user account is active (extension) | `true` |
| Bio | string | Short biography (extension, from UserProfile) | `Technology writer with 10+ years of experience...` |
| AvatarUrl | string | URL to user's profile picture (extension, from UserProfile) | `https://storage.propulse.com/avatars/johndoe.jpg` |

#### UserSocialMediaHandle

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| Id | string | Primary key | `5408a5d6-672b-4df6-9205-008ee51b19a6` |
| UserId | string | Foreign key to ApplicationUser | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| Platform | string | Social media platform name | `Twitter` |
| Handle | string | User's handle on the platform | `@johndoe` |
| Url | string | Full URL to the user's profile on the platform | `https://twitter.com/johndoe` |
| IsPublic | bool | Whether the handle is visible to other users | `true` |
| CreatedAt | DateTimeOffset | When the handle was created (DB-managed) | `2025-04-03T10:15:30Z` |
| UpdatedAt | DateTimeOffset | When the handle was last updated (DB-managed) | `2025-04-03T14:22:15Z` |

### 1.2 Entity Relationships

The Identity Service manages user authentication, authorization, and profile information through several related entities:

```mermaid
erDiagram
    ApplicationUser ||--o{ UserSocialMediaHandle : "has"
    ApplicationUser ||--o{ IdentityUserRole : "has"
    IdentityRole ||--o{ IdentityUserRole : "has"
    ApplicationUser ||--o{ IdentityUserClaim : "has"
    ApplicationUser ||--o{ IdentityUserLogin : "has"
    ApplicationUser ||--o{ IdentityUserToken : "has"
    
    ApplicationUser {
        string Id PK
        string UserName
        string Email
        bool EmailConfirmed
        string PhoneNumber
        bool PhoneNumberConfirmed
        bool TwoFactorEnabled
        DateTimeOffset LockoutEnd
        bool LockoutEnabled
        int AccessFailedCount
        string DisplayName
        DateTimeOffset CreatedAt
        DateTimeOffset LastLoginAt
        bool IsActive
        string Bio
        string AvatarUrl
    }
    
    UserSocialMediaHandle {
        int Id PK
        string UserId FK
        string Platform
        string Handle
        string Url
        bool IsPublic
    }
    
    IdentityRole {
        string Id PK
        string Name
        string NormalizedName
    }
    
    IdentityUserRole {
        string UserId PK,FK
        string RoleId PK,FK
    }
    
    IdentityUserClaim {
        int Id PK
        string UserId FK
        string ClaimType
        string ClaimValue
    }
    
    IdentityUserLogin {
        string LoginProvider PK
        string ProviderKey PK
        string UserId FK
        string ProviderDisplayName
    }
    
    IdentityUserToken {
        string UserId PK,FK
        string LoginProvider PK
        string Name PK
        string Value
    }
```

### 1.3 OAuth 2.0 and OIDC Implementation

#### Identity Provider Configuration
- OpenID Connect server implementation using ASP.NET Core Identity and OpenIddict
- Support for authorization code flow with PKCE
- Support for refresh tokens
- Support for external identity providers (Google, Microsoft, Facebook, etc.)
- Token signing using ES256 (preferred) or RS256 algorithm
- JWT token configuration with appropriate claims and scopes
- Centralized user store with PostgreSQL for production

#### Client Applications
- Reading Interface: Server-side authentication using cookie authentication
- Publishing Interface: SPA client using Authorization Code flow with PKCE
- API Gateway: JWT validation middleware for securing backend services

### 1.4 API Endpoints

#### Authentication & Authorization
- `/.well-known/openid-configuration`: OpenID Connect discovery endpoint
- `/.well-known/jwks.json`: JSON Web Key Set endpoint
- `/connect/authorize`: Authorization endpoint
- `/connect/token`: Token endpoint
- `/connect/userinfo`: UserInfo endpoint
- `/connect/logout`: Logout endpoint
- `/connect/revoke`: Token revocation endpoint
- `/connect/introspect`: Token introspection endpoint

#### User Management
- `GET /api/identity/users/me`: Get current user details
- `PUT /api/identity/users/me`: Update current user details
- `POST /api/identity/users/register`: Register a new user (for self-service registration)
- `POST /api/identity/users/confirm-email`: Confirm user email
- `POST /api/identity/users/forgot-password`: Initiate password reset
- `POST /api/identity/users/reset-password`: Complete password reset

#### Social Media Authentication
- `GET /api/identity/external-login/{provider}`: Initiate external login
- `GET /api/identity/external-login-callback`: Handle external login callback
- `POST /api/identity/external-login-confirmation`: Link external login to existing account

### 1.5 Error Handling
- Invalid email format: Return 400 with a message indicating the email format is invalid
- Username already exists: Return 409 with a message indicating the username is already taken
- Invalid credentials: Return 401 with a generic authentication failure message
- Unauthorized access: Return 403 when a user tries to access/modify another user's data
- Invalid token: Return 401 with appropriate error code as per OAuth 2.0 specification
- Invalid grant: Return 400 with appropriate error code as per OAuth 2.0 specification
- Server errors during authentication: Return 500 with a correlation ID for troubleshooting

### 1.6 Testing Strategy

#### Unit Tests
- Test user registration with valid and invalid data
- Test authentication with correct and incorrect credentials
- Test token validation and refresh logic
- Test user profile update with valid and invalid data
- Test JWT creation and validation
- Test claims transformation and authorization policies

#### Integration Tests
- Test the full authentication flow with each supported grant type
- Test authorization policies for different user types
- Test integration with external identity providers
- Test token refresh and revocation
- Test integration with other services that depend on identity

### 1.7 API Documentation

The Identity Service will expose its API documentation following the OpenAPI specification:

- OpenAPI document exposed at `/openapi/index.json`
- Swagger UI available at `/swagger` for non-production environments
- API versioning using URL path versioning (e.g., `/api/v1/identity/...`)
- Documentation will include OAuth 2.0 security scheme definitions
- All API endpoints will be documented with descriptions, request/response schemas, and examples
- Authentication and authorization requirements will be clearly specified for each endpoint

## 2. Content Service

### 2.1 Data Model

#### Article

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| Id | string | Unique identifier for the article | `7fa85f64-5717-4562-b3fc-2c963f66afa8` |
| Title | string | Article title | `"10 Best Practices for Social Media Marketing"` |
| Content | string | The main content of the article in HTML format | `"<h1>Introduction</h1><p>Social media has become...</p>"` |
| AuthorId | string | Foreign key to ApplicationUser | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| Status | enum | Draft, Published, Archived | `Published` |
| PublishedAt | DateTimeOffset? | When the article was or will be published | `2025-04-15T09:00:00Z` |
| CreatedAt | DateTimeOffset | When the article was created (DB-managed) | `2025-04-03T10:15:30Z` |
| UpdatedAt | DateTimeOffset | When the article was last modified (DB-managed) | `2025-04-03T14:22:15Z` |
| Summary | string | Brief summary of the article | `"Learn the top 10 practices that will improve your social media marketing strategy."` |
| Slug | string | URL-friendly version of the title | `"10-best-practices-for-social-media-marketing"` |
| FeaturedImageUrl | string | URL to the featured image | `"https://storage.propulse.com/articles/social-media-best-practices.jpg"` |

#### Category

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| Id | string | Unique identifier for the category | `9fa85f64-5717-4562-b3fc-2c963f66afa9` |
| Name | string | Category name | `"Social Media Marketing"` |
| Description | string | Category description | `"Articles related to social media marketing strategies and best practices"` |
| Slug | string | URL-friendly version of the name | `"social-media-marketing"` |
| CreatedAt | DateTimeOffset | When the category was created (DB-managed) | `2025-04-03T10:15:30Z` |
| UpdatedAt | DateTimeOffset | When the category was last modified (DB-managed) | `2025-04-03T14:22:15Z` |

#### Tag

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| Id | string | Unique identifier for the tag | `1fa85f64-5717-4562-b3fc-2c963f66afb0` |
| Name | string | Tag name | `"Instagram"` |
| Slug | string | URL-friendly version of the name | `"instagram"` |
| CreatedAt | DateTimeOffset | When the tag was created (DB-managed) | `2025-04-03T10:15:30Z` |
| UpdatedAt | DateTimeOffset | When the tag was last modified (DB-managed) | `2025-04-03T14:22:15Z` |

#### ArticleCategory

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| ArticleId | string | Foreign key to Article | `7fa85f64-5717-4562-b3fc-2c963f66afa8` |
| CategoryId | string | Foreign key to Category | `9fa85f64-5717-4562-b3fc-2c963f66afa9` |

#### ArticleTag

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| ArticleId | string | Foreign key to Article | `7fa85f64-5717-4562-b3fc-2c963f66afa8` |
| TagId | string | Foreign key to Tag | `1fa85f64-5717-4562-b3fc-2c963f66afb0` |

### 2.2 Entity Relationships

The Content Service manages articles, categories, and tags through several related entities:

```mermaid
erDiagram
    Article ||--o{ ArticleCategory : "belongs to"
    Article ||--o{ ArticleTag : "has"
    Category ||--o{ ArticleCategory : "contains"
    Tag ||--o{ ArticleTag : "applied to"
    ApplicationUser ||--o{ Article : "authors"
    
    Article {
        string Id PK
        string Title
        string Content
        string AuthorId FK
        enum Status
        DateTimeOffset PublishDate
        DateTimeOffset CreatedAt
        DateTimeOffset UpdatedAt
        string Summary
        string Slug
        string FeaturedImageUrl
    }
    
    Category {
        string Id PK
        string Name
        string Description
        string Slug
        DateTimeOffset CreatedAt
        DateTimeOffset UpdatedAt
    }
    
    Tag {
        string Id PK
        string Name
        string Slug
        DateTimeOffset CreatedAt
        DateTimeOffset UpdatedAt
    }
    
    ArticleCategory {
        string ArticleId PK,FK
        string CategoryId PK,FK
    }
    
    ArticleTag {
        string ArticleId PK,FK
        string TagId PK,FK
    }
    
    ApplicationUser {
        string Id PK
        string DisplayName
    }
```

### 2.3 API Endpoints

#### Articles
- `GET /api/content/articles`: Get a list of articles with pagination and filtering
- `GET /api/content/articles/{id}`: Get a specific article by ID
- `GET /api/content/articles/slug/{slug}`: Get a specific article by slug
- `POST /api/content/articles`: Create a new article
- `PUT /api/content/articles/{id}`: Update an existing article
- `DELETE /api/content/articles/{id}`: Delete an article (mark as archived)
- `POST /api/content/articles/{id}/publish`: Publish an article
- `POST /api/content/articles/{id}/unpublish`: Unpublish an article

#### Categories & Tags
- `GET /api/content/categories`: Get all categories
- `GET /api/content/categories/{id}/articles`: Get articles for a specific category
- `GET /api/content/tags`: Get all tags
- `GET /api/content/tags/{id}/articles`: Get articles for a specific tag
- `POST /api/content/categories`: Create a new category
- `POST /api/content/tags`: Create a new tag

### 2.4 Error Handling
- Article not found: Return 404 with a message that the article doesn't exist
- Invalid article data: Return 400 with validation errors
- Unauthorized access: Return 403 when a user tries to modify an article they don't own
- Publication errors: Return 422 with specific error messages for publication issues

### 2.5 Testing Strategy

#### Unit Tests
- Test article creation with valid and invalid data
- Test article publication workflow
- Test category and tag association logic
- Test slug generation and uniqueness

#### Integration Tests
- Test full article creation, editing, and publication flow
- Test search and filtering functionality
- Test integration with the Identity service for author information

### 2.6 API Documentation

The Content Service will expose its API documentation following the OpenAPI specification:

- OpenAPI document exposed at `/openapi/index.json`
- Swagger UI available at `/swagger` for non-production environments
- API versioning using URL path versioning (e.g., `/api/v1/content/...`)
- Documentation will include security scheme definitions (JWT Bearer)
- All API endpoints will be documented with descriptions, request/response schemas, and examples
- Example request/response pairs for key operations like article creation and publishing

## 3. Community Service

### 3.1 Data Model

#### Comment

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| Id | string | Unique identifier for the comment | `2fa85f64-5717-4562-b3fc-2c963f66afb1` |
| ArticleId | string | Foreign key to Article | `7fa85f64-5717-4562-b3fc-2c963f66afa8` |
| UserId | string | Foreign key to ApplicationUser | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| Content | string | The comment text | `"Great article! I especially liked the section about Instagram best practices."` |
| CreatedAt | DateTimeOffset | When the comment was created (DB-managed) | `2025-04-03T16:45:30Z` |
| UpdatedAt | DateTimeOffset | When the comment was last modified (DB-managed) | `2025-04-03T16:55:30Z` |
| IsDeleted | bool | Soft deletion flag | `false` |
| ParentCommentId | string | Foreign key to parent Comment for nested comments | `null` (for top-level comment) |

#### Rating

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| ArticleId | string | Foreign key to Article (part of composite primary key) | `7fa85f64-5717-4562-b3fc-2c963f66afa8` |
| UserId | string | Foreign key to ApplicationUser (part of composite primary key) | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| Value | int | Rating value (1-5) | `4` |
| CreatedAt | DateTimeOffset | When the rating was created (DB-managed) | `2025-04-03T16:45:30Z` |
| UpdatedAt | DateTimeOffset | When the rating was last modified (DB-managed) | `2025-04-03T16:45:30Z` |

### 3.2 Entity Relationships

The Community Service manages user interactions with content through comments and ratings:

```mermaid
erDiagram
    Comment ||--o{ Comment : "parent of"
    Article ||--o{ Comment : "has"
    ApplicationUser ||--o{ Comment : "writes"
    Article ||--o{ Rating : "rated by"
    ApplicationUser ||--o{ Rating : "rates"
    
    Comment {
        string Id PK
        string ArticleId FK
        string UserId FK
        string Content
        DateTimeOffset CreatedAt
        DateTimeOffset UpdatedAt
        bool IsDeleted
        string ParentCommentId FK
    }
    
    Rating {
        string ArticleId PK,FK
        string UserId PK,FK
        int Value
        DateTimeOffset CreatedAt
        DateTimeOffset UpdatedAt
    }
    
    Article {
        string Id PK
        string Title
    }
    
    ApplicationUser {
        string Id PK
        string DisplayName
    }
```

### 3.3 API Endpoints

#### Comments
- `GET /api/community/articles/{articleId}/comments`: Get comments for an article
- `POST /api/community/articles/{articleId}/comments`: Add a comment to an article
- `PUT /api/community/comments/{id}`: Update a comment
- `DELETE /api/community/comments/{id}`: Delete a comment
- `POST /api/community/comments/{id}/reply`: Reply to a comment

#### Ratings
- `GET /api/community/articles/{articleId}/ratings`: Get the average rating and count for an article
- `POST /api/community/articles/{articleId}/ratings`: Rate an article
- `PUT /api/community/articles/{articleId}/ratings`: Update a rating
- `DELETE /api/community/articles/{articleId}/ratings`: Remove a rating

### 3.4 Error Handling
- Comment not found: Return 404 when a comment doesn't exist
- Invalid rating: Return 400 when a rating is outside the acceptable range
- Unauthorized: Return 403 when a user tries to modify another user's comment
- Already rated: Return 409 when a user tries to rate an article more than once

### 3.5 Testing Strategy

#### Unit Tests
- Test comment creation, editing, and deletion
- Test rating validation and averaging
- Test comment threading functionality
- Test integration with Identity service for user information

#### Integration Tests
- Test complete comment and reply workflow
- Test rating calculation and aggregation
- Test authorization for comment and rating operations

### 3.6 API Documentation

The Community Service will expose its API documentation following the OpenAPI specification:

- OpenAPI document exposed at `/openapi/index.json`
- Swagger UI available at `/swagger` for non-production environments
- API versioning using URL path versioning (e.g., `/api/v1/community/...`)
- Documentation will include security scheme definitions (JWT Bearer)
- All API endpoints will be documented with descriptions, request/response schemas, and examples
- Example request/response pairs for comment creation and rating operations

## 4. Distribution Service (Minimal for Phase 1)

### 4.1 Data Model

#### SocialMediaAccount

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| Id | string | Unique identifier for the social media account | `4fa85f64-5717-4562-b3fc-2c963f66afb2` |
| UserId | string | Foreign key to ApplicationUser | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| Platform | enum | Twitter, Facebook, LinkedIn, Instagram | `Twitter` |
| AccessToken | string | OAuth access token (encrypted) | `"gho_16C7e42F292c6912E7710c838347Ae178B4a"` |
| RefreshToken | string | OAuth refresh token (encrypted) | `"ghr_1B4a2e77838347a7E420ce178F2E7c6912E1R38"` |
| TokenExpiresAt | DateTimeOffset | When the access token expires | `2025-05-03T16:45:30Z` |
| CreatedAt | DateTimeOffset | When the account was created (DB-managed) | `2025-04-03T10:15:30Z` |
| UpdatedAt | DateTimeOffset | When the account was last modified (DB-managed) | `2025-04-03T14:22:15Z` |
| IsActive | bool | Whether the connection is active | `true` |

#### SocialMediaShare

| Column Name | Type | Description | Example |
|------------|------|-------------|---------|
| Id | string | Unique identifier for the share | `6fa85f64-5717-4562-b3fc-2c963f66afb3` |
| ArticleId | string | Foreign key to Article | `7fa85f64-5717-4562-b3fc-2c963f66afa8` |
| SocialMediaAccountId | string | Foreign key to SocialMediaAccount | `4fa85f64-5717-4562-b3fc-2c963f66afb2` |
| Content | string | The text content of the share | `"Check out my new article: 10 Best Practices for Social Media Marketing"` |
| ShareUrl | string | URL to the shared post on the platform | `"https://twitter.com/johndoe/status/1234567890"` |
| SharedAt | DateTimeOffset? | When the article was shared | `2025-04-03T16:45:30Z` |
| CreatedAt | DateTimeOffset | When the share record was created (DB-managed) | `2025-04-03T16:40:30Z` |
| UpdatedAt | DateTimeOffset | When the share record was last updated (DB-managed) | `2025-04-03T16:45:30Z` |
| Status | enum | Pending, Shared, Failed | `Shared` |
| ErrorMessage | string | Error message if sharing failed | `"Rate limit exceeded"` |

### 4.2 Entity Relationships

The Distribution Service manages social media connections and shares:

```mermaid
erDiagram
    SocialMediaAccount ||--o{ SocialMediaShare : "used for"
    ApplicationUser ||--o{ SocialMediaAccount : "owns"
    Article ||--o{ SocialMediaShare : "shared via"
    
    SocialMediaAccount {
        string Id PK
        string UserId FK
        enum Platform
        string AccessToken
        string RefreshToken
        DateTimeOffset TokenExpiresAt
        DateTimeOffset CreatedAt
        DateTimeOffset UpdatedAt
        bool IsActive
    }
    
    SocialMediaShare {
        string Id PK
        string ArticleId FK
        string SocialMediaAccountId FK
        string Content
        string ShareUrl
        DateTimeOffset SharedAt
        DateTimeOffset CreatedAt
        DateTimeOffset UpdatedAt
        enum Status
        string ErrorMessage
    }
    
    ApplicationUser {
        string Id PK
        string DisplayName
    }
    
    Article {
        string Id PK
        string Title
    }
```

### 4.3 API Endpoints

#### Social Media Accounts
- `GET /api/distribution/accounts`: Get user's social media accounts
- `POST /api/distribution/accounts/{platform}/connect`: Connect to a social media platform
- `DELETE /api/distribution/accounts/{id}`: Disconnect a social media account

#### Manual Sharing (for Phase 1)
- `POST /api/distribution/articles/{articleId}/share`: Share an article now to connected platforms
- `GET /api/distribution/articles/{articleId}/shares`: Get sharing history for an article

### 4.4 Error Handling
- Connection expired: Return 401 when OAuth tokens are expired
- Platform error: Return 502 when the social media platform returns an error
- Account not connected: Return 404 when trying to share to a non-connected account
- Article not published: Return 400 when trying to share an unpublished article

### 4.5 Testing Strategy

#### Unit Tests
- Test OAuth token management and refresh logic
- Test share formatting for different platforms
- Test error handling for various API responses

#### Integration Tests
- Test OAuth connection flow with mock providers
- Test manual sharing workflow
- Test error handling and retries

### 4.6 API Documentation

The Distribution Service will expose its API documentation following the OpenAPI specification:

- OpenAPI document exposed at `/openapi/index.json`
- Swagger UI available at `/swagger` for non-production environments
- API versioning using URL path versioning (e.g., `/api/v1/distribution/...`)
- Documentation will include security scheme definitions (JWT Bearer)
- All API endpoints will be documented with descriptions, request/response schemas, and examples
- Example request/response pairs for social media connection and sharing operations

## 5. API Gateway

### 5.1 Configuration
- Route configuration for all backend services
- Authentication and authorization middleware
- CORS policy configuration
- Rate limiting rules
- Request/response logging

### 5.2 Endpoints
The API Gateway will expose all the endpoints from the individual services under a unified domain:
- `/api/identity/*`: Routes to Identity Service
- `/api/content/*`: Routes to Content Service
- `/api/community/*`: Routes to Community Service
- `/api/distribution/*`: Routes to Distribution Service

### 5.3 Error Handling
- Service unavailable: Return 503 when a downstream service is unavailable
- Request timeout: Return 504 when a service takes too long to respond
- Bad gateway: Return 502 when a service returns an unexpected response

### 5.4 Testing Strategy
- Test routing to all services
- Test authentication and authorization flows
- Test error handling and failover scenarios
- Test rate limiting functionality

### 5.5 API Documentation

The API Gateway will provide a comprehensive view of all microservice APIs:

- **Composite OpenAPI Document**: The API Gateway will aggregate OpenAPI documents from all microservices and expose a unified OpenAPI specification at `/openapi/index.json`
- **Swagger UI**: Available at `/swagger` for non-production environments, showing all available endpoints across services
- **Service Discovery**: The gateway will dynamically discover and include API documentation from registered microservices
- **API Explorer**: Integrated API testing interface for developers
- **Documentation Generation**: Automatic generation of HTML documentation from the OpenAPI specifications

### 5.6 Developer Experience

For development environments, the API Gateway will integrate with Scalar to enhance the developer experience:

- **Scalar Integration**: Provides an enhanced API exploration experience over standard Swagger UI
- **Request Builder**: Visual interface for building and testing API requests
- **Authentication Flow Testing**: Built-in support for testing OAuth flows
- **Response Visualization**: Better formatting and visualization of API responses
- **Mock Server**: Support for mocking responses based on the OpenAPI specification
- **Development Workflow**: Integration with the development environment for streamlined API testing

#### Configuration for Development Environment

```json
{
  "ApiGateway": {
    "SwaggerUI": {
      "Enabled": true,
      "Route": "/swagger"
    },
    "Scalar": {
      "Enabled": true,
      "Route": "/scalar",
      "Theme": "default",
      "Authentication": {
        "ClientId": "propulse-dev",
        "Scopes": ["openid", "profile", "email", "api"]
      }
    },
    "OpenApi": {
      "DocumentPath": "/openapi/index.json",
      "Version": "v1",
      "Title": "ProPulse API",
      "Description": "API for the ProPulse blogging and social media marketing platform"
    }
  }
}
```

## 6. Reading Interface (Razor Pages)

### 6.1 Page Structure
- `Index.cshtml`: Homepage with featured and recent articles
- `Articles/Index.cshtml`: Article listing with search and filtering
- `Articles/Details.cshtml`: Article reading page with comments and rating
- `Categories/Index.cshtml`: Categories listing page
- `Categories/Details.cshtml`: Articles in a specific category
- `Tags/Index.cshtml`: Tags listing page
- `Tags/Details.cshtml`: Articles with a specific tag
- `Account/Register.cshtml`: User registration page
- `Account/Login.cshtml`: User login page
- `Account/Profile.cshtml`: User profile page

### 6.2 Client-Side Assets
- Basic CSS framework (responsive grid, typography, utilities)
- JavaScript for comments, ratings, and search functionality
- Article reading progress tracking
- Social media sharing buttons

### 6.3 Error Handling
- 404 Not Found page for missing articles/categories/tags
- Error pages for 400, 403, 500 status codes
- Client-side form validation
- Loading states for asynchronous operations

### 6.4 Testing Strategy
- UI tests for critical flows (reading, commenting, rating)
- Browser compatibility testing
- Responsive design testing
- Accessibility testing

### 6.5 API Integration

The Reading Interface will integrate with backend services through the API Gateway:

- **Authentication**: Server-side cookie-based authentication via Identity Service's OpenID Connect endpoints
- **Article Retrieval**: Content Service API calls for article content and metadata
- **Comments and Ratings**: Community Service API calls for user interactions
- **Social Sharing**: Distribution Service API calls for article sharing capabilities

### 6.6 Performance Optimization

- Server-side rendering for optimal SEO
- Content caching strategy with appropriate cache headers
- Lazy loading of images and embedded media
- Minification and bundling of static assets
- Critical CSS inlining for above-the-fold content

## 7. Publishing Interface (Single Page Application)

### 7.1 Page Structure
- `Login`: Authentication page for content creators
- `Dashboard`: Overview with article statistics
- `Articles`: List of articles with filtering and sorting
- `Articles/Editor`: WYSIWYG article editor with preview
- `Articles/Settings`: Article metadata and publishing options
- `Categories`: Category management
- `Tags`: Tag management
- `Profile`: User profile management
- `SocialMedia`: Connected social media accounts
- `SocialMedia/Share`: Manual sharing interface

### 7.2 Components
- Rich text editor with image upload
- Article preview component
- Date and time pickers for scheduling
- Tag and category selector components
- Social media account connection widgets

### 7.3 Error Handling
- Form validation for all input fields
- Error handling for API failures
- Auto-saving drafts to prevent data loss
- Conflict resolution for simultaneous edits

### 7.4 Testing Strategy
- UI tests for critical flows (article creation, publishing)
- Component unit tests
- Responsive design testing
- Performance testing for the rich text editor

### 7.5 API Integration

The Publishing Interface will integrate with backend services through the API Gateway:

- **Authentication**: OAuth 2.0 Authorization Code flow with PKCE for secure SPA authentication
- **Article Management**: Content Service API calls for CRUD operations on articles
- **User Management**: Identity Service API calls for profile management
- **Social Media Integration**: Distribution Service API calls for connecting and managing social accounts

### 7.6 Technical Implementation

- Single-page application built with Blazor WebAssembly
- State management with Fluxor for predictable state containers
- Component-based architecture for reusability and maintainability
- Strong typing with C# for improved developer experience and compile-time safety
- Responsive design supporting desktop and tablet experiences
- MudBlazor component library for consistent UI elements
- Progressive Web App (PWA) capabilities for offline editing
- Integration with browser APIs for file uploads and clipboard operations
- WebSockets for real-time collaboration features in future phases

## 8. Data Storage & Persistence

### 8.1 Databases

#### PostgreSQL Configuration

| Environment | Configuration | Description |
|-------------|--------------|-------------|
| Production | Managed PostgreSQL on Azure | High-availability setup with read replicas |
| Staging | Managed PostgreSQL on Azure | Smaller instance than production |
| Development | Local PostgreSQL container | Docker-based for developer convenience |
| Testing | SQLite | In-memory for unit tests, file-based for integration tests |

#### Database Schema Management

- DbUp-based migration approach with separate projects for each microservice database
- Idempotent SQL scripts versioned in the repository
- Migration scripts executed automatically during service startup in development
- Separate deployment pipeline for database migrations in production environments
- Scripted rollback procedures for each migration

#### Database Project Structure

```
/src
  /Database
    /ProPulse.Database.Identity
      /Scripts
        /0001_Initial
          0001_Create_Users_Table.sql
          0002_Create_Roles_Table.sql
          0003_Create_UserRoles_Table.sql
        /0002_AddSocialMedia
          0001_Create_UserSocialMediaHandles_Table.sql
      Program.cs
    /ProPulse.Database.Content
      /Scripts
        /0001_Initial
          0001_Create_Articles_Table.sql
          0002_Create_Categories_Table.sql
          0003_Create_Tags_Table.sql
          0004_Create_ArticleCategories_Table.sql
          0005_Create_ArticleTags_Table.sql
      Program.cs
    /ProPulse.Database.Community
      /Scripts
        /0001_Initial
          0001_Create_Comments_Table.sql
          0002_Create_Ratings_Table.sql
      Program.cs
    /ProPulse.Database.Distribution
      /Scripts
        /0001_Initial
          0001_Create_SocialMediaAccounts_Table.sql
          0002_Create_SocialMediaShares_Table.sql
      Program.cs
```

#### Example DbUp Implementation

```csharp
/// <summary>
/// Main entry point for the database migration utility
/// </summary>
public static class Program
{
    /// <summary>
    /// Executes database migrations using DbUp
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Exit code indicating success or failure</returns>
    public static int Main(string[] args)
    {
        // Get connection string from command line args or environment variables
        var connectionString = GetConnectionString(args);
        
        // Ensure database exists
        EnsureDatabase.For.PostgresqlDatabase(connectionString);
        
        // Create upgrader with all SQL scripts embedded in the assembly
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransaction()
            .LogToConsole()
            .Build();
            
        // Execute the migrations
        var result = upgrader.PerformUpgrade();
        
        // Check for success
        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
            return -1;
        }
        
        // Log success
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Database migration successful!");
        Console.ResetColor();
        return 0;
    }
    
    /// <summary>
    /// Gets the connection string from command line args or environment variables
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Connection string to use for database operations</returns>
    private static string GetConnectionString(string[] args)
    {
        // Check command line args first
        if (args.Length > 0)
        {
            return args[0];
        }
        
        // Then check environment variables
        var envVarName = "DATABASE_CONNECTION_STRING";
        var connectionString = Environment.GetEnvironmentVariable(envVarName);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"No connection string provided. Pass it as a command line argument or set the {envVarName} environment variable.");
        }
        
        return connectionString;
    }
}
```

#### Integration with .NET Aspire

```csharp
// In the AppHost project

/// <summary>
/// Adds database with migrations for a microservice
/// </summary>
/// <param name="builder">The distributed application builder</param>
/// <param name="name">Name of the database</param>
/// <param name="migrationProject">Migration project name</param>
/// <returns>The PostgreSQL resource</returns>
private static IResourceBuilder<PostgresResource> AddDatabaseWithMigrations(
    this DistributedApplicationBuilder builder, 
    string name, 
    string migrationProject)
{
    // Add PostgreSQL database resource
    var postgres = builder.AddPostgres(name)
        .WithEnvironment("POSTGRES_PASSWORD", builder.Configuration["DB_PASSWORD"] ?? "devpassword");
    
    // Add migration project as a dependency
    var migration = builder.AddProject<Projects.Database>(migrationProject)
        .WithEnvironment("DATABASE_CONNECTION_STRING", postgres.GetConnectionString());
    
    // Configure the database to depend on successful migration
    postgres.WithAnnotation(new ResourceDependencyAnnotation(migration));
    
    return postgres;
}

// Usage in Aspire AppHost
var identityDb = builder.AddDatabaseWithMigrations("identity", "ProPulse.Database.Identity");
var contentDb = builder.AddDatabaseWithMigrations("content", "ProPulse.Database.Content");
var communityDb = builder.AddDatabaseWithMigrations("community", "ProPulse.Database.Community");
var distributionDb = builder.AddDatabaseWithMigrations("distribution", "ProPulse.Database.Distribution");
```

### 8.2 Entity Framework Core Configuration

#### Common Base Entity

```csharp
/// <summary>
/// Base class for all entities in the system providing common properties
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public string Id { get; set; } = default!;
    
    /// <summary>
    /// Timestamp when the entity was created (managed by DB trigger)
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when the entity was last updated (managed by DB trigger)
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
    
    /// <summary>
    /// Concurrency token used for optimistic concurrency control and ETag generation.
    /// This value is automatically updated by database triggers.
    /// </summary>
    [ConcurrencyCheck]
    public byte[] RowVersion { get; set; } = new byte[8];
}
```

#### Entity Configuration Example

```csharp
/// <summary>
/// Configuration for the Article entity in Entity Framework Core
/// </summary>
public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    /// <summary>
    /// Configures the Article entity with its properties, indexes, and relationships
    /// </summary>
    /// <param name="builder">The entity type builder to use for configuration</param>
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");
            
        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(a => a.Slug)
            .IsRequired()
            .HasMaxLength(250);
            
        builder.HasIndex(a => a.Slug)
            .IsUnique();
            
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
            
        // Trigger setup for CreatedAt and UpdatedAt will be handled by database migrations
    }
}
```

### 8.3 Media Storage

#### Azure Blob Storage Configuration

| Storage Type | Purpose | Lifecycle Policy |
|-------------|--------|-----------------|
| Hot Storage | Active article images | Move to cool storage after 90 days of no access |
| Cool Storage | Archive article images | Move to archive storage after 1 year of no access |
| Archive Storage | Long-term backup | No deletion, compliance retention |

#### CDN Integration

- Azure CDN for global content delivery
- Custom domain mapping
- HTTPS enforcement
- Browser caching optimization
- Automatic compression

### 8.4 Data Access Patterns

- Repository pattern for data access abstraction
- Unit of Work pattern for transaction management
- CQRS pattern for read/write segregation in high-traffic services
- Specification pattern for complex query composition

### 8.5 Row Versioning and Conditional Request Support

To support RFC 9110 conditional requests across all Web APIs, each microservice will implement consistent row versioning and ETag generation.

#### Database Triggers for Row Versioning

PostgreSQL and SQLite triggers will be used to automatically update the RowVersion field when a record is modified.

##### PostgreSQL Trigger

```sql
-- Create a function to update row_version
CREATE OR REPLACE FUNCTION update_row_version()
RETURNS TRIGGER AS $$
BEGIN
    -- Generate a new random 8-byte value for row_version on each update
    NEW.row_version = decode(md5(random()::text || clock_timestamp()::text), 'hex');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger for each table 
-- Example for Articles table
CREATE TRIGGER trg_articles_row_version
BEFORE UPDATE ON articles
FOR EACH ROW
EXECUTE FUNCTION update_row_version();
```

##### SQLite Trigger

```sql
-- SQLite implementation for row versioning
-- For each table, e.g., Articles
CREATE TRIGGER trg_articles_row_version 
AFTER UPDATE ON articles
BEGIN
    -- Use randomblob to generate a new 8-byte value 
    UPDATE articles 
    SET row_version = randomblob(8) 
    WHERE id = NEW.id;
END;
```

#### ETag Generation and Validation

```csharp
/// <summary>
/// Extensions for working with ETag and conditional request headers
/// </summary>
public static class EntityTagExtensions
{
    /// <summary>
    /// Generates a strong ETag value from the entity's row version
    /// </summary>
    /// <param name="rowVersion">The binary row version from the entity</param>
    /// <returns>A formatted ETag string including quotes</returns>
    public static string GenerateETag(byte[] rowVersion)
    {
        // Convert the binary row version to a Base64 string
        string base64Version = Convert.ToBase64String(rowVersion);
        
        // Return a strong ETag (no W/ prefix) with required quotes
        return $"\"{base64Version}\"";
    }
    
    /// <summary>
    /// Extracts the row version from an ETag value
    /// </summary>
    /// <param name="etag">The ETag value from a request header</param>
    /// <returns>The decoded row version as a byte array, or null if invalid</returns>
    public static byte[]? GetRowVersionFromETag(string? etag)
    {
        if (string.IsNullOrEmpty(etag))
        {
            return null;
        }
        
        // Remove quotes from ETag
        string trimmedETag = etag.Trim('"');
        
        try
        {
            // Convert from Base64 back to binary
            return Convert.FromBase64String(trimmedETag);
        }
        catch (FormatException)
        {
            // Return null if the ETag is not properly formatted
            return null;
        }
    }
}
```

#### Middleware for Conditional Request Support

```csharp
/// <summary>
/// Middleware to add RFC 9110 conditional request support to API endpoints
/// </summary>
public class ConditionalRequestMiddleware
{
    private readonly RequestDelegate _next;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalRequestMiddleware"/> class
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    public ConditionalRequestMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    /// <summary>
    /// Processes the request to add conditional request support
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Only handle GET, HEAD, PUT, PATCH, DELETE methods
        if (context.Request.Method is not "GET" and not "HEAD" and not "PUT" and not "PATCH" and not "DELETE")
        {
            await _next(context);
            return;
        }
        
        // Capture the response to intercept entity responses
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        // Process the request through the pipeline
        await _next(context);
        
        // Handle conditional responses based on HTTP method and status code
        if (context.Response.StatusCode == 200 && context.Items.TryGetValue("EntityLastModified", out var lastModified) 
            && context.Items.TryGetValue("EntityETag", out var etag))
        {
            var lastModifiedDate = (DateTimeOffset)lastModified;
            var etagValue = (string)etag;
            
            // Add headers to the response
            context.Response.Headers.ETag = etagValue;
            context.Response.Headers.LastModified = lastModifiedDate.ToString("R");
            
            // Check If-None-Match for GET/HEAD requests
            if ((context.Request.Method == "GET" || context.Request.Method == "HEAD") && 
                context.Request.Headers.IfNoneMatch.Count > 0)
            {
                var ifNoneMatch = context.Request.Headers.IfNoneMatch.ToString();
                if (ifNoneMatch == etagValue || ifNoneMatch == "*")
                {
                    // Resource not modified, return 304
                    context.Response.StatusCode = 304;
                    context.Response.Body = originalBodyStream;
                    return;
                }
            }
            
            // Check If-Modified-Since for GET/HEAD requests
            if ((context.Request.Method == "GET" || context.Request.Method == "HEAD") && 
                context.Request.Headers.IfModifiedSince.HasValue)
            {
                var ifModifiedSince = context.Request.Headers.IfModifiedSince.Value;
                if (lastModifiedDate <= ifModifiedSince)
                {
                    // Resource not modified, return 304
                    context.Response.StatusCode = 304;
                    context.Response.Body = originalBodyStream;
                    return;
                }
            }
            
            // For state-changing requests (PUT, PATCH, DELETE), check If-Match
            if ((context.Request.Method == "PUT" || context.Request.Method == "PATCH" || context.Request.Method == "DELETE") && 
                context.Request.Headers.IfMatch.Count > 0)
            {
                var ifMatch = context.Request.Headers.IfMatch.ToString();
                if (ifMatch != "*" && ifMatch != etagValue)
                {
                    // Precondition failed, return 412
                    context.Response.StatusCode = 412;
                    context.Response.Body = originalBodyStream;
                    return;
                }
            }
            
            // Check If-Unmodified-Since for state-changing requests
            if ((context.Request.Method == "PUT" || context.Request.Method == "PATCH" || context.Request.Method == "DELETE") && 
                context.Request.Headers.IfUnmodifiedSince.HasValue)
            {
                var ifUnmodifiedSince = context.Request.Headers.IfUnmodifiedSince.Value;
                if (lastModifiedDate > ifUnmodifiedSince)
                {
                    // Precondition failed, return 412
                    context.Response.StatusCode = 412;
                    context.Response.Body = originalBodyStream;
                    return;
                }
            }
        }
        
        // Copy the response body to the original stream and restore it
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }
}

/// <summary>
/// Extensions for enabling conditional request support in ASP.NET Core
/// </summary>
public static class ConditionalRequestExtensions
{
    /// <summary>
    /// Adds conditional request support for RFC 9110 compliance
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for method chaining</returns>
    public static IApplicationBuilder UseConditionalRequests(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ConditionalRequestMiddleware>();
    }
    
    /// <summary>
    /// Sets the entity information for conditional request handling
    /// </summary>
    /// <typeparam name="T">Type of the entity implementing IEntityWithRowVersion</typeparam>
    /// <param name="context">The HTTP context</param>
    /// <param name="entity">The entity with row version information</param>
    public static void SetEntityInfo<T>(this HttpContext context, T entity) where T : BaseEntity
    {
        if (entity == null)
        {
            return;
        }
        
        context.Items["EntityLastModified"] = entity.UpdatedAt;
        context.Items["EntityETag"] = EntityTagExtensions.GenerateETag(entity.RowVersion);
    }
}
```

## 12. Cross-Service Data Consistency Management

### 12.1 Handling Cascading Deletes Across Microservices

In our microservice architecture, traditional database CASCADE DELETE operations cannot work across service boundaries. To maintain data consistency when a resource (like a user or article) is deleted, we'll implement a multi-phase deletion strategy.

#### Soft Delete Implementation

All major entities across services will implement a soft delete pattern with an `IsDeleted` flag and a `DeletedAt` timestamp:

```csharp
/// <summary>
/// Base class for entities that support soft deletion
/// </summary>
public abstract class SoftDeleteEntity : BaseEntity
{
    /// <summary>
    /// Flag indicating if the entity has been soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Timestamp when the entity was soft deleted, if applicable
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
```

#### Event-Driven Deletion Process

For cascading deletes across services, we will use a combination of the Outbox pattern and Event-Driven Architecture:

##### Entity Configuration Example

```csharp
/// <summary>
/// Configuration for the Article entity in Entity Framework Core with soft delete
/// </summary>
public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    /// <summary>
    /// Configures the Article entity with properties, indexes, relationships, and query filters
    /// </summary>
    /// <param name="builder">The entity type builder to use for configuration</param>
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        // ...existing configuration code...
        
        // Configure global query filter for soft delete
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
```

##### Cross-Service Delete Event Flow

When a user or article is deleted:

1. The service that owns the resource (Identity or Content) performs a soft delete
2. It publishes a deletion event to a message broker (Azure Service Bus)
3. Other services subscribe to these events and perform their own soft deletes
4. A background process handles permanent deletion after a retention period

The following diagram illustrates the flow:

```mermaid
sequenceDiagram
    participant API as API Gateway
    participant Identity as Identity Service
    participant Content as Content Service
    participant Community as Community Service
    participant Distribution as Distribution Service
    participant EventBus as Azure Service Bus

    API ->> Identity: DELETE /api/identity/users/{id}
    Identity ->> Identity: Soft Delete User
    Identity -->> EventBus: Publish UserDeleted Event
    Identity ->> API: 200 OK
    
    EventBus -->> Content: Receive UserDeleted Event
    Content ->> Content: Soft Delete User's Articles
    Content -->> EventBus: Publish ArticlesDeleted Event
    
    EventBus -->> Community: Receive UserDeleted Event
    Community ->> Community: Soft Delete User's Comments & Ratings
    
    EventBus -->> Distribution: Receive UserDeleted Event
    Distribution ->> Distribution: Soft Delete User's Social Media Accounts & Shares
    
    EventBus -->> Community: Receive ArticlesDeleted Event
    Community ->> Community: Soft Delete Article's Comments & Ratings
    
    EventBus -->> Distribution: Receive ArticlesDeleted Event
    Distribution ->> Distribution: Soft Delete Article's Social Media Shares
```

### 12.2 Implementation Details

#### Outbox Pattern

To ensure reliable event publishing even if a service's message broker is temporarily unavailable:

```csharp
/// <summary>
/// Represents a message to be published to the event bus
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Unique identifier for the message
    /// </summary>
    public string Id { get; set; } = default!;
    
    /// <summary>
    /// The type of event being published
    /// </summary>
    public string EventType { get; set; } = default!;
    
    /// <summary>
    /// The serialized content of the event
    /// </summary>
    public string Content { get; set; } = default!;
    
    /// <summary>
    /// When the outbox message was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// When the message was processed, if it has been
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; set; }
    
    /// <summary>
    /// Number of attempts to process this message
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// Error message if processing has failed
    /// </summary>
    public string? Error { get; set; }
}
```

#### Delete Event Handlers

Each service will implement event handlers for entity deletion:

```csharp
/// <summary>
/// Handles user deletion events from the Identity Service
/// </summary>
public class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
{
    private readonly IArticleRepository _articleRepository;
    private readonly ILogger<UserDeletedEventHandler> _logger;
    private readonly IPublisher _publisher;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UserDeletedEventHandler"/> class
    /// </summary>
    /// <param name="articleRepository">Repository for article data access</param>
    /// <param name="logger">Logger for diagnostic information</param>
    /// <param name="publisher">Publisher for downstream events</param>
    public UserDeletedEventHandler(
        IArticleRepository articleRepository,
        ILogger<UserDeletedEventHandler> logger,
        IPublisher publisher)
    {
        _articleRepository = articleRepository;
        _logger = logger;
        _publisher = publisher;
    }
    
    /// <summary>
    /// Handles the user deleted event by soft-deleting associated articles
    /// </summary>
    /// <param name="event">The user deleted event</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task HandleAsync(UserDeletedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing user deletion for user {UserId}", @event.UserId);
        
        // Get all articles by this user
        var articles = await _articleRepository.GetByAuthorIdAsync(@event.UserId, cancellationToken);
        
        // Soft delete each article
        foreach (var article in articles)
        {
            article.IsDeleted = true;
            article.DeletedAt = DateTimeOffset.UtcNow;
            await _articleRepository.UpdateAsync(article, cancellationToken);
            
            // Publish article deleted event for downstream services
            await _publisher.PublishAsync(new ArticleDeletedEvent 
            { 
                ArticleId = article.Id,
                DeletedAt = article.DeletedAt.Value
            }, cancellationToken);
        }
        
        _logger.LogInformation("Completed processing user deletion for {UserId}, soft-deleted {Count} articles", 
            @event.UserId, articles.Count);
    }
}
```

### 12.3 Hard Deletion Process

For permanent deletion after a retention period, a scheduled task in each service will:

1. Query for soft-deleted entities older than the retention period
2. Perform hard deletion of these entities from the database
3. Archive necessary data for compliance and auditing purposes

```csharp
/// <summary>
/// Background service that processes hard deletion of expired soft-deleted entities
/// </summary>
public class HardDeletionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HardDeletionService> _logger;
    private readonly IConfiguration _configuration;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HardDeletionService"/> class
    /// </summary>
    /// <param name="serviceProvider">Service provider for scoped service creation</param>
    /// <param name="logger">Logger for diagnostic information</param>
    /// <param name="configuration">Application configuration</param>
    public HardDeletionService(
        IServiceProvider serviceProvider,
        ILogger<HardDeletionService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }
    
    /// <summary>
    /// Executes the background task that processes hard deletion
    /// </summary>
    /// <param name="stoppingToken">Token that can be used to stop the task</param>
    /// <returns>A task representing the background operation</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Get retention period from config (default 90 days)
        int retentionDays = _configuration.GetValue<int>("DataRetention:SoftDeletedRetentionDays", 90);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessHardDeletionsAsync(retentionDays, stoppingToken);
                
                // Run once a day
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred during hard deletion processing");
                
                // Back off before retrying
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
    
    /// <summary>
    /// Processes hard deletion for expired soft-deleted entities
    /// </summary>
    /// <param name="retentionDays">Number of days to retain soft-deleted data</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task ProcessHardDeletionsAsync(int retentionDays, CancellationToken cancellationToken)
    {
        // Calculate cutoff date
        DateTimeOffset cutoffDate = DateTimeOffset.UtcNow.AddDays(-retentionDays);
        
        using var scope = _serviceProvider.CreateScope();
        
        // Get scoped services for repository access
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Process entities in batches to avoid memory issues
        int batchSize = 100;
        int totalProcessed = 0;
        
        // Example for Articles
        var expiredArticles = await context.Articles
            .Where(a => a.IsDeleted && a.DeletedAt < cutoffDate)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
            
        while (expiredArticles.Any())
        {
            // Archive data if needed for compliance
            // await ArchiveEntitiesAsync(expiredArticles, cancellationToken);
            
            // Remove from database
            context.Articles.RemoveRange(expiredArticles);
            await context.SaveChangesAsync(cancellationToken);
            
            totalProcessed += expiredArticles.Count;
            
            // Get next batch
            expiredArticles = await context.Articles
                .Where(a => a.IsDeleted && a.DeletedAt < cutoffDate)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }
        
        _logger.LogInformation("Hard deletion process completed. Removed {Count} expired articles", totalProcessed);
    }
}
```

### 12.4 Phased Implementation for MVP

For the initial MVP phase, we'll implement:

1. Soft deletion across all services
2. Basic event publication for deletion events
3. Event handlers for cascading soft deletes

The hard deletion process will be deferred to a later phase, allowing us to:

- Gather data on deletion patterns
- Refine the retention policy based on actual usage
- Implement a more sophisticated archiving strategy if needed
- Add additional compliance and audit features

This phased approach lets us deliver a working solution quickly while setting the foundation for a more comprehensive data lifecycle management system in future phases.

### 12.5 Local Development with Containerized Message Broker

To support the development paradigm where everything is runnable locally using .NET Aspire, we'll implement a containerized message broker alternative to Azure Service Bus for local development.

#### RabbitMQ Integration with Aspire

For local development, we'll use RabbitMQ as our containerized message broker:

```csharp
// In the AppHost project
var builder = DistributedApplication.CreateBuilder(args);

// Add RabbitMQ container for local development
var rabbitmq = builder.AddRabbitMQContainer("rabbitmq", password: "guest");

// Add services with reference to RabbitMQ
var identityService = builder.AddProject<Projects.IdentityServiceProject>("identity-service")
    .WithReference(rabbitmq);

var contentService = builder.AddProject<Projects.ContentServiceProject>("content-service")
    .WithReference(rabbitmq);

// Add other services...
```

#### Message Bus Abstraction Layer

To maintain consistent code across environments, we'll implement a messaging abstraction:

```csharp
namespace ProPulse.Shared.Messaging;

/// <summary>
/// Interface for message bus operations
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publishes a message to the specified topic
    /// </summary>
    /// <typeparam name="T">Type of message to publish</typeparam>
    /// <param name="topic">Topic name to publish to</param>
    /// <param name="message">Message content to publish</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribes to messages from the specified topic
    /// </summary>
    /// <typeparam name="T">Type of message to receive</typeparam>
    /// <param name="topic">Topic name to subscribe to</param>
    /// <param name="handler">Handler function for received messages</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SubscribeAsync<T>(string topic, Func<T, Task> handler, CancellationToken cancellationToken = default);
}
```

#### Environment-Specific Implementation

We'll register the appropriate implementation based on the environment:

```csharp
// In each service's Program.cs
builder.Services.AddSingleton<IMessageBus>(serviceProvider =>
{
    IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
    IHostEnvironment environment = serviceProvider.GetRequiredService<IHostEnvironment>();
    ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    if (environment.IsDevelopment())
    {
        // Use RabbitMQ in development
        logger.LogInformation("Configuring RabbitMQ message bus for development");
        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };
        
        return new RabbitMQMessageBus(
            factory, 
            serviceProvider, 
            serviceProvider.GetRequiredService<ILogger<RabbitMQMessageBus>>());
    }
    else
    {
        // Use Azure Service Bus in other environments
        logger.LogInformation("Configuring Azure Service Bus for production");
        string connectionString = configuration["AzureServiceBus:ConnectionString"] 
            ?? throw new InvalidOperationException("Azure Service Bus connection string is not configured");
            
        return new AzureServiceBusMessageBus(
            connectionString, 
            serviceProvider, 
            serviceProvider.GetRequiredService<ILogger<AzureServiceBusMessageBus>>());
    }
});
```

#### RabbitMQ Implementation Example

The RabbitMQ implementation of the message bus abstraction:

```csharp
namespace ProPulse.Shared.Messaging;

/// <summary>
/// RabbitMQ implementation of IMessageBus for development environment
/// </summary>
public class RabbitMQMessageBus : IMessageBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQMessageBus> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQMessageBus"/> class
    /// </summary>
    /// <param name="connectionFactory">Factory to create RabbitMQ connections</param>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <param name="logger">Logger for diagnostic information</param>
    public RabbitMQMessageBus(
        ConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQMessageBus> logger)
    {
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
    {
        _channel.ExchangeDeclare(exchange: topic, type: ExchangeType.Fanout, durable: true);
        
        string json = JsonSerializer.Serialize(message);
        byte[] body = Encoding.UTF8.GetBytes(json);
        
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        
        _channel.BasicPublish(
            exchange: topic,
            routingKey: string.Empty,
            basicProperties: properties,
            body: body);
            
        _logger.LogInformation("Published message to {Topic}", topic);
        
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SubscribeAsync<T>(string topic, Func<T, Task> handler, CancellationToken cancellationToken = default)
    {
        _channel.ExchangeDeclare(exchange: topic, type: ExchangeType.Fanout, durable: true);
        
        string queueName = $"{topic}_{typeof(T).Name}_{Guid.NewGuid()}";
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: queueName, exchange: topic, routingKey: string.Empty);
        
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string json = Encoding.UTF8.GetString(body);
            
            try
            {
                T? receivedMessage = JsonSerializer.Deserialize<T>(json);
                if (receivedMessage != null)
                {
                    await handler(receivedMessage);
                }
                
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {Topic}", topic);
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };
        
        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        
        return Task.CompletedTask;
    }
}
```

This approach allows developers to run the entire system locally using .NET Aspire with containerized RabbitMQ, while maintaining code compatibility with the Azure Service Bus implementation used in staging and production environments.
# MyChat - Real-time Chat Application

A modern chat application built with .NET C# backend and React TypeScript frontend.

## Features

### Current Implementation ✅
- **User Authentication**: Register and login with JWT tokens
- **Input Validation**: Form validation with error handling
- **Raw Password Storage**: Simple password storage (as requested for hackathon)
- **PostgreSQL Database**: User data persistence
- **RESTful API**: Clean API design with proper HTTP status codes
- **CORS Configuration**: Ready for frontend integration

### Planned Features 🚧
- Real-time messaging with SignalR
- 1-on-1 conversations
- Group chats (up to 300 participants)
- Contact management
- Image sharing
- Message search
- Message reactions

## Quick Start

### Prerequisites
- .NET 8 SDK
- PostgreSQL database
- Node.js 18+ (for frontend)

### Backend Setup

1. **Clone and navigate to backend**:
```bash
git clone <repository>
cd hackathon-2025/backend
```

2. **Install dependencies**:
```bash
dotnet restore
```

3. **Configure database connection**:
   Update `appsettings.json` with your PostgreSQL connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mychat;Username=postgres;Password=your_password"
  }
}
```

4. **Run database migrations**:
```bash
dotnet ef database update
```

5. **Run the application**:
```bash
dotnet run
```

The API will be available at `https://localhost:5001`

### API Endpoints

#### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `GET /api/auth/check-username/{username}` - Check username availability

#### Request/Response Examples

**Register**:
```bash
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "password": "password123",
  "confirmPassword": "password123"
}
```

**Response**:
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 1,
      "username": "john_doe",
      "createdAt": "2025-05-31T10:00:00Z"
    }
  },
  "errors": []
}
```

**Login**:
```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "password123"
}
```

## Project Structure

```
backend/
├── Controllers/          # API controllers
│   └── AuthController.cs
├── Entities/            # Database entities
│   └── User.cs
├── Models/DTOs/         # Data Transfer Objects
│   ├── Auth/
│   └── Common/
├── Services/            # Business logic services
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   ├── IJwtService.cs
│   └── JwtService.cs
├── Data/               # Database context
│   └── ChatDbContext.cs
├── Configuration/      # Configuration models
│   └── JwtSettings.cs
├── Extensions/         # Service extensions
│   └── ServiceCollectionExtensions.cs
├── Migrations/         # EF Core migrations
└── Program.cs          # Application entry point
```

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 9.0
- **Language**: C#
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Real-time**: SignalR (planned)

### Frontend (Planned)
- **Framework**: React 18
- **Language**: TypeScript
- **State Management**: React Context/Redux
- **Real-time**: SignalR JavaScript client
- **Styling**: Tailwind CSS or styled-components

## Development

### Build Commands
```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Run with hot reload
dotnet watch run
```

### Database Commands
```bash
# Add migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Security Features

- **JWT Authentication**: Secure token-based authentication
- **Input Validation**: Server-side validation with data annotations
- **CORS Policy**: Configured for frontend integration
- **Raw Password Storage**: Simplified for hackathon (not for production)

## Environment Variables

Configure the following in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your PostgreSQL connection string"
  },
  "JwtSettings": {
    "SecretKey": "Your JWT secret key (minimum 32 characters)",
    "Issuer": "MyChat",
    "Audience": "MyChat",
    "ExpiryInMinutes": 1440
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

## Testing the API

You can test the API using tools like Postman, curl, or the included Swagger UI (available in development mode at `/swagger`).

Example curl commands:

```bash
# Register a new user
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"password123","confirmPassword":"password123"}'

# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"password123"}'

# Check username availability
curl https://localhost:5001/api/auth/check-username/testuser
```

## Next Steps

1. **Frontend Development**: React TypeScript application
2. **Real-time Messaging**: SignalR hub implementation
3. **Message Persistence**: Message entity and service
4. **Contact Management**: Friend/contact system
5. **Group Chats**: Multi-user conversation support
6. **File Upload**: Image sharing functionality
7. **Deployment**: Docker containerization and cloud deployment

## Contributing

This is a hackathon project. Focus on core functionality and rapid development.

## License

This project is created for hackathon purposes.

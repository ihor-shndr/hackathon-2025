# Tech Context

## Technology Stack Decisions

### Backend Technology Options
**Selected: .NET C# (ASP.NET Core)**
- **Rationale**: Excellent performance, mature ecosystem, strong typing, built-in WebSocket support
- **Alternatives**: Node.js + TypeScript, Java (Spring Boot)
- **Key Libraries**:
  - ASP.NET Core Web API for REST endpoints
  - SignalR for real-time WebSocket communication
  - Entity Framework Core for database ORM
  - JWT Bearer authentication
  - BCrypt.Net for password hashing

### Frontend Technology
**Recommended: React + TypeScript**
- **Rationale**: Component-based UI, excellent real-time updates, large ecosystem
- **Alternatives**: Vue.js, Angular, or Blazor Server/WebAssembly
- **Key Libraries**:
  - SignalR JavaScript client for real-time communication
  - React Router for navigation
  - Axios for HTTP requests
  - Styled-components or Tailwind CSS for styling

### Database
**Required: PostgreSQL**
- **Rationale**: Contest requirement, excellent for transactional integrity
- **Features Used**:
  - JSONB for flexible message metadata
  - Full-text search for message history
  - Indexing for performance optimization
  - Transactions for data consistency
- **Entity Framework Core**: Code-first migrations and LINQ queries

### Cloud Storage
**Image Storage: Cloud Blob Storage**
- **AWS**: S3 buckets
- **Azure**: Blob Storage
- **GCP**: Cloud Storage
- **Local Development**: MinIO for S3-compatible local storage

## Development Environment Setup

### Prerequisites
```bash
# Required tools
- .NET 8 SDK
- Node.js 18+ and npm (for frontend)
- Docker and Docker Compose
- Git
- PostgreSQL (for local development)
```

### Project Structure
```
hackathon-2025/
├── frontend/           # React TypeScript app
│   ├── src/
│   ├── public/
│   ├── package.json
│   └── Dockerfile
├── backend/            # ASP.NET Core C# API
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Hubs/          # SignalR hubs
│   ├── Data/          # Entity Framework context
│   ├── Migrations/    # EF Core migrations
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Backend.csproj
│   └── Dockerfile
├── infrastructure/     # IaC deployment scripts
│   ├── docker-compose.yml
│   ├── deploy.sh
│   └── destroy.sh
├── README.md
└── .env.example
```

### Build Commands (Contest Requirements)
```bash
# 1-2 commands to build after git clone
./build.sh             # Install dependencies and build both frontend and backend

# Alternative commands
dotnet restore && dotnet build backend/    # Backend only
npm install && npm run build --prefix frontend/  # Frontend only
```

### Local Development
```bash
# Single command to run locally
docker compose up

# Alternative for development
dotnet run --project backend/  # Backend on https://localhost:5001
npm start --prefix frontend/   # Frontend on http://localhost:3000
```

## Infrastructure Requirements

### Container Configuration
**Docker Compose Setup**:
```yaml
version: '3.8'
services:
  frontend:
    build: ./frontend
    ports:
      - "3000:3000"
    depends_on:
      - backend
  
  backend:
    build: ./backend
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Database=chatapp;Username=user;Password=pass
      - JwtSettings__SecretKey=${JWT_SECRET}
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      - db
  
  db:
    image: postgres:15
    environment:
      POSTGRES_DB: chatapp
      POSTGRES_USER: user
      POSTGRES_PASSWORD: pass
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  postgres_data:
```

### Cloud Deployment Architecture
**Infrastructure as Code (Terraform recommended)**:
```hcl
# Example AWS deployment structure
- ECS Fargate for container orchestration
- RDS PostgreSQL for database
- S3 for image storage
- ALB for load balancing
- CloudWatch for monitoring
```

### Performance Targets & Constraints

**Concurrent Users**: 500-1000
- WebSocket connection pooling
- Horizontal scaling capability
- Database connection limits

**Message Throughput**: 50 messages/second
- Efficient database writes
- Real-time broadcasting optimization
- Message queuing for high load

**Response Time Requirements**:
- Message delivery: < 100ms
- Chat history loading: < 500ms
- Image upload: < 2 seconds

## Development Tools & Workflow

### Code Quality Tools
```json
{
  "devDependencies": {
    "@typescript-eslint/eslint-plugin": "^6.0.0",
    "prettier": "^3.0.0",
    "husky": "^8.0.0",
    "lint-staged": "^14.0.0",
    "jest": "^29.0.0",
    "@testing-library/react": "^13.0.0"
  }
}
```

### Environment Configuration
```bash
# Required environment variables (.NET appsettings.json)
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=chatapp;Username=user;Password=pass"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "ChatApp",
    "Audience": "ChatApp",
    "ExpiryInMinutes": 60
  },
  "CloudStorage": {
    "BucketName": "your-bucket-name",
    "Region": "us-east-1"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Testing Strategy
**Unit Tests**: xUnit for C# business logic, Jest for React components
**Integration Tests**: ASP.NET Core Test Host for API testing
**E2E Tests**: Playwright for critical user flows (optional due to time constraint)

## Security Considerations

### Authentication & Session Management
- JWT tokens with appropriate expiration
- Secure password hashing (bcrypt, cost factor 12+)
- HTTPS enforcement in production
- CORS configuration for frontend-backend communication

### Data Validation
- Input sanitization for all user data
- File upload validation (type, size limits)
- SQL injection prevention via ORM
- XSS prevention in message rendering

### Rate Limiting
```csharp
// Example rate limiting configuration in ASP.NET Core
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("MessagePolicy", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });
});
```

## Real-Time Communication Setup

### SignalR Configuration
```csharp
// SignalR hub setup in Program.cs
builder.Services.AddSignalR();

// Configure CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Map SignalR hub
app.MapHub<ChatHub>("/chathub");
```

### Fallback Strategy
- **Primary**: WebSocket for real-time communication
- **Fallback**: HTTP polling every 2-3 seconds
- **Detection**: Automatic fallback on connection failure

## Deployment & CI/CD

### Cloud Provider Strategy
**Cloud Agnostic Design**:
- Use environment variables for cloud-specific configuration
- Abstract storage operations behind interfaces
- Containerized deployment for portability

### IaC Deployment Scripts
```bash
#!/bin/bash
# deploy.sh - One command cloud deployment
set -e

echo "Deploying chat application..."
terraform init
terraform plan
terraform apply -auto-approve

echo "Deployment complete!"
echo "Application URL: $(terraform output app_url)"
```

```bash
#!/bin/bash
# destroy.sh - One command cleanup
set -e

echo "Destroying infrastructure..."
terraform destroy -auto-approve
echo "Cleanup complete!"
```

### Monitoring & Observability
**Basic Requirements**:
- Application logs (structured JSON)
- Database connection monitoring
- WebSocket connection metrics
- Error tracking and alerting

**Tools**:
- Console logging for development
- Cloud provider native monitoring (CloudWatch, Azure Monitor)
- Optional: Application Performance Monitoring (APM) tools

## Development Timeline Considerations

### 1-Day Contest Constraints
**Priority 1 (Core Features)**:
- User authentication
- Basic messaging (1-on-1)
- Real-time communication
- Message persistence

**Priority 2 (Enhanced Features)**:
- Group chats
- Contact management
- Image sharing

**Priority 3 (Advanced Features)**:
- Message search
- Reactions
- Performance optimization

### Quick Development Strategies
- Use create-react-app for rapid frontend setup
- Leverage .NET project templates for backend structure
- Use Entity Framework Core migrations for rapid database schema iteration
- Docker Compose for integrated development environment
- Pre-built UI component libraries for faster interface development
- SignalR for simplified real-time communication setup

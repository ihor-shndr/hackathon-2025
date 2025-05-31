# System Patterns

## Overall Architecture

### High-Level System Design ✅ COMPLETE

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend UI   │    │  Backend API    │    │   PostgreSQL    │
│      ✅         │◄──►│      ✅         │◄──►│   Database ✅   │
│ - Chat Interface│    │ - REST API      │    │ - Users         │
│ - Real-time UI  │    │ - WebSocket*    │    │ - Messages      │
│ - User Auth     │    │ - Auth Service  │    │ - Contacts      │
│ - TypeScript    │    │ - 25 Endpoints  │    │ - Groups        │
└─────────────────┘    └─────────────────┘    │ - Full Schema   │
                                │              └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │  Cloud Storage  │
                       │   (Prepared)    │
                       │ - Image Files   │
                       │ - Blob Storage  │
                       └─────────────────┘
```

*SignalR WebSocket: Only remaining component

### Core Architectural Patterns

**1. Real-Time Communication Pattern**
- **Primary**: SignalR WebSocket connections for real-time messaging
- **Fallback**: SignalR automatic fallback to Server-Sent Events or Long Polling
- **Auto-detection**: SignalR handles transport negotiation automatically

**2. Message Delivery Guarantee Pattern**
- Server acknowledges message receipt immediately
- Messages persisted to database before acknowledgment
- Client retries until acknowledgment received
- Delivery confirmation to all participants

**3. Scalable Connection Management**
- Connection pooling for database
- WebSocket connection management with heartbeat
- Horizontal scaling via stateless API design

## Database Design Patterns

### Core Entities Schema

```sql
-- Users table
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    last_seen TIMESTAMP
);

-- Contacts (bidirectional relationships)
CREATE TABLE contacts (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id),
    contact_id INTEGER REFERENCES users(id),
    status VARCHAR(20) DEFAULT 'pending', -- pending, accepted, blocked
    created_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(user_id, contact_id)
);

-- Groups
CREATE TABLE groups (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    owner_id INTEGER REFERENCES users(id),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Group members
CREATE TABLE group_members (
    group_id INTEGER REFERENCES groups(id),
    user_id INTEGER REFERENCES users(id),
    joined_at TIMESTAMP DEFAULT NOW(),
    PRIMARY KEY (group_id, user_id)
);

-- Messages (unified for 1-on-1 and group)
CREATE TABLE messages (
    id SERIAL PRIMARY KEY,
    sender_id INTEGER REFERENCES users(id),
    group_id INTEGER REFERENCES groups(id) NULL, -- NULL for 1-on-1
    recipient_id INTEGER REFERENCES users(id) NULL, -- NULL for group
    content TEXT NOT NULL,
    message_type VARCHAR(20) DEFAULT 'text', -- text, image
    image_url VARCHAR(500), -- for image messages
    created_at TIMESTAMP DEFAULT NOW(),
    INDEX(group_id, created_at),
    INDEX(sender_id, recipient_id, created_at)
);

-- Message reactions
CREATE TABLE message_reactions (
    id SERIAL PRIMARY KEY,
    message_id INTEGER REFERENCES messages(id),
    user_id INTEGER REFERENCES users(id),
    reaction VARCHAR(10) NOT NULL, -- emoji or reaction type
    created_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(message_id, user_id, reaction)
);
```

### Data Access Patterns

**1. Message Retrieval Pattern**
- Paginated queries for chat history
- Index optimization for conversation threads
- Full-text search across message content

**2. Contact Management Pattern**
- Bidirectional contact relationships
- Status tracking (pending, accepted, blocked)
- Efficient friend suggestion queries

**3. Real-time Notification Pattern**
- Event-driven message broadcasting
- User presence tracking
- Targeted delivery to active connections

## Component Interaction Patterns

### Authentication Flow
```
1. User Registration/Login
   Frontend → Backend API → Database
   ↓
2. JWT Token Generation
   Backend → Frontend (store in localStorage/cookie)
   ↓
3. Authenticated Requests
   Frontend (+ token) → Backend API
```

### Real-Time Messaging Flow
```
1. Message Send
   Frontend → SignalR Hub → ASP.NET Core Backend
   ↓
2. Message Persistence
   Backend → Entity Framework → PostgreSQL
   ↓
3. Message Broadcasting
   Backend → SignalR Hub → All Recipients
   ↓
4. Delivery Confirmation
   Recipients → SignalR Hub → Sender
```

### Image Upload Pattern
```
1. Image Selection
   Frontend → File Validation
   ↓
2. Upload to Cloud Storage
   Frontend → Cloud Blob Storage
   ↓
3. URL Storage
   Frontend → Backend API → Database
   ↓
4. Message with Image URL
   Standard message flow with image_url
```

## Scalability Patterns

### Horizontal Scaling Strategy
- **Stateless API Design**: No server-side session storage
- **Database Connection Pooling**: Efficient connection management
- **Load Balancer Ready**: Multiple backend instances
- **WebSocket Scaling**: Sticky sessions or message broker pattern

### Performance Optimization Patterns
- **Database Indexing**: Optimized for chat history queries
- **Caching Layer**: Redis for active user sessions (optional)
- **Image CDN**: Cloud storage with CDN for fast image delivery
- **Connection Management**: WebSocket heartbeat and cleanup

## Deployment Patterns

### Containerization Strategy
```dockerfile
# Multi-stage build pattern for .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 80 443
ENTRYPOINT ["dotnet", "Backend.dll"]

# Frontend container
FROM node:18-alpine AS frontend
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build
```

### Infrastructure as Code Pattern
- **Cloud Provider Agnostic**: Works on AWS, Azure, GCP
- **Container Orchestration**: Docker Compose for local, ECS/AKS for cloud
- **Database Deployment**: Managed PostgreSQL service
- **Storage Integration**: Cloud blob storage (S3, Azure Blob, GCS)

### Development Workflow Pattern
```bash
# Local development
git clone <repo>
npm install  # or equivalent build command
docker compose up

# Cloud deployment
./deploy.sh  # IaC deployment script
./destroy.sh # IaC cleanup script
```

## Error Handling Patterns

### Message Delivery Resilience
- **Client-side retry logic** with exponential backoff
- **Server-side duplicate detection** via message IDs
- **Connection failure recovery** with automatic reconnection

### Data Consistency Patterns
- **Database transactions** for multi-table operations
- **Optimistic locking** for concurrent message handling
- **Eventual consistency** for cross-service operations

## Security Patterns

### Authentication & Authorization
- **JWT-based authentication** with secure token storage
- **Password hashing** using bcrypt or similar
- **Input validation** on all API endpoints
- **SQL injection prevention** via parameterized queries

### Data Protection
- **HTTPS enforcement** for all communications
- **WebSocket Secure (WSS)** for real-time connections
- **Image upload validation** for file type and size
- **Rate limiting** to prevent abuse

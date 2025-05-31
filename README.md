# MyChat - Real-time Chat Application

A complete Skype-like chat application built for the DataArt hackathon contest, featuring real-time messaging, group chats, and user management.

## 🚀 Quick Start

### Local Development (Recommended)

Run the complete application with Docker Compose:

```bash
# Clone the repository
git clone <repository-url>
cd hackathon-2025

# Start the application (one command!)
docker compose up --build
```

**Application URLs:**
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:8080
- **Database**: localhost:5432

That's it! The application is ready to use.

### Manual Local Setup

If you prefer running services individually:

```bash
# 1. Start database
docker compose up db -d

# 2. Start backend
cd backend
dotnet run

# 3. Start frontend (new terminal)
cd frontend
npm install
npm start
```

## ☁️ Cloud Deployment

**Note: Cloud deployment infrastructure is prepared but not fully operational yet.**

The application includes Terraform configuration for AWS deployment:

```bash
# Deploy to cloud (when ready)
cd terraform
./deploy.sh

# Destroy cloud resources
./destroy.sh
```

**Planned Cloud Components:**
- AWS ECS Fargate for containers
- RDS PostgreSQL for database
- S3 for image storage
- Application Load Balancer

## 🎯 Features

### ✅ Implemented
- **Real-time Messaging**: Instant direct and group chat using SignalR
- **User Authentication**: Secure registration and login with JWT
- **Contact Management**: Send invites, accept/decline requests
- **Group Chats**: Create groups, manage members, group messaging
- **Image Sharing**: Upload and share images in chats
- **Message History**: Persistent chat history with search
- **Responsive UI**: Modern design that works on all devices

### 🏗️ Architecture
- **Backend**: .NET 8 Web API with SignalR for real-time communication
- **Frontend**: React 18 with TypeScript and Tailwind CSS
- **Database**: PostgreSQL with Entity Framework Core
- **Real-time**: WebSocket connections with automatic fallback

## 📋 Requirements

**For Local Development:**
- Docker and Docker Compose
- Git

**For Manual Setup (optional):**
- Node.js 18+
- .NET 8 SDK
- PostgreSQL 15+

## 🛠️ Development

### Project Structure
```
hackathon-2025/
├── backend/           # .NET Web API + SignalR
├── frontend/          # React TypeScript app
├── infrastructure/    # Docker Compose setup
├── terraform/         # AWS deployment (in progress)
└── memory-bank/       # Project documentation
```

### Environment Configuration

**Default Local Settings (Docker):**
- Database: `postgres://postgres:postgres@db:5432/mychat`
- API URL: `http://localhost:8080`
- Frontend: `http://localhost:3000`

**Key Environment Variables:**
```bash
# Frontend
REACT_APP_API_URL=http://localhost:8080

# Backend
ConnectionStrings__DefaultConnection=Host=db;Database=mychat;Username=postgres;Password=postgres
ASPNETCORE_ENVIRONMENT=Development
```

## 🧪 Testing the Application

1. **Start the application**: `docker compose up --build`
2. **Open browser**: Navigate to http://localhost:3000
3. **Register users**: Create multiple user accounts
4. **Test real-time chat**: 
   - Open multiple browser tabs with different users
   - Send messages and see them appear instantly
   - Test group creation and messaging

## 🐛 Troubleshooting

**Common Issues:**

1. **Port conflicts**: Ensure ports 3000, 8080, and 5432 are available
2. **Docker issues**: 
   ```bash
   docker compose down -v  # Reset volumes
   docker compose up --build
   ```
3. **Database connection**: Wait for health checks to pass
4. **Frontend not loading**: Check if backend is running on port 8080

**Logs:**
```bash
# View all logs
docker compose logs

# View specific service logs
docker compose logs backend
docker compose logs frontend
docker compose logs db
```

## 🏆 Hackathon Compliance

**Contest Requirements Met:**
- ✅ Buildable in 1-2 commands after `git clone`
- ✅ Runnable with `docker compose up`
- ✅ Real-time messaging functionality
- ✅ User registration and authentication
- ✅ Group chat capabilities
- ✅ Message persistence
- ✅ Modern web UI
- 🔄 Cloud deployment (infrastructure ready, not operational)

## ⚠️ Known Limitations

Based on hackathon requirements, the following features are **not implemented**:

### Missing Core Features
- **Message Formatting**: Bold and italic text formatting not implemented
- **Message Reactions**: Emoji reactions to messages not implemented
- **Contact Info View**: Contact details viewing not implemented
- **Message Search**: Cross-chat search functionality not implemented
- **Persistent URLs**: Direct links to specific chats/groups not implemented

### Missing Advanced Features
- **Performance Testing Tools**: No load testing utilities included
- **WebSocket Fallback**: Automatic fallback to HTTP polling not implemented
- **Up to 300 group participants**: Current limit not tested
- **Message delivery confirmation**: Read receipts not implemented

### Infrastructure Limitations
- **Cloud Deployment**: Infrastructure prepared but not fully operational
- **Image Storage**: S3 integration prepared but requires AWS configuration
- **Production Scaling**: Not tested for 500-1000 concurrent users
- **Message Throughput**: Not tested for 50 messages/second requirement

## 📄 License

MIT License - Built for DataArt hackathon contest 2025.

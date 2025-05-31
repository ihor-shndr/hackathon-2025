# MyChat - Real-time Chat Application

A complete chat application built for the hackathon contest, featuring real-time messaging, group chats, and user management.

## 🌟 **LIVE DEMO**

**🔗 Try the live application: http://mychat-app-alb-1160711057.us-east-1.elb.amazonaws.com**

✅ **Fully operational on AWS ECS** - Register, add contacts, and start real-time chatting!

---

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

**✅ LIVE: http://mychat-app-alb-1160711057.us-east-1.elb.amazonaws.com**

The application is successfully deployed on AWS using Terraform:

```bash
# Deploy to cloud
cd terraform
./deploy.sh

# Destroy cloud resources
./destroy.sh
```

**✅ Operational Cloud Components:**
- ✅ AWS ECS Fargate for containers - **RUNNING**
- ✅ RDS PostgreSQL for database - **ACTIVE**
- ✅ S3 for image storage - **FUNCTIONAL**
- ✅ Application Load Balancer - **DISTRIBUTING TRAFFIC**

## 🎯 Features

### ✅ Implemented
- **Real-time Messaging**: Instant direct and group chat using SignalR
- **User Authentication**: Secure registration and login with JWT
- **Contact Management**: Send invites, accept/decline requests, instant chat access
- **Group Chats**: Create groups, manage members, group messaging
- **Image Sharing**: Upload and share images in chats (S3 integration working)
- **Message Formatting**: Bold and italic text formatting with visual toolbar
- **Message History**: Persistent chat history
- **Modern Dark Theme UI**: Professional design that works on all devices
- **Cloud Deployment**: Live on AWS ECS with full functionality

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
├── terraform/         # AWS deployment (operational)
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
- ✅ Modern web UI with professional dark theme
- ✅ **Cloud deployment LIVE and operational**
- ✅ Image sharing with S3 integration
- ✅ Message formatting capabilities

**🌟 LIVE DEMO: http://mychat-app-alb-1160711057.us-east-1.elb.amazonaws.com**

## ✅ Current Status

### ✅ **Fully Implemented Features**
- **✅ Real-time Messaging**: SignalR WebSocket connections working
- **✅ User Authentication**: JWT-based secure auth system
- **✅ Contact Management**: Complete invitation flow with instant chat access
- **✅ Group Chats**: Full group creation and management
- **✅ Image Sharing**: S3 integration fully operational
- **✅ Message Formatting**: Bold and italic text with visual toolbar
- **✅ Modern UI**: Professional dark theme design
- **✅ Cloud Deployment**: Live on AWS ECS - **OPERATIONAL**

### ⚠️ **Future Enhancements**
- Message reactions and emoji support
- Advanced search functionality
- Performance optimization for high load
- Mobile app development

## 📄 License

MIT License - Built for DataArt hackathon contest 2025.

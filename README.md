# MyChat Application

A real-time chat application built with .NET Core backend and React TypeScript frontend.

## Quick Start

### Option 1: Docker Compose (Recommended)

Run the entire application stack (frontend, backend, and database) with one command:

```bash
# Navigate to infrastructure directory
cd infrastructure

# Start all services
./start.sh
# or manually:
# docker-compose up --build

# Stop all services
./stop.sh
# or manually:
# docker-compose down
```

After starting, the application will be available at:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:8080
- **Database**: localhost:5432

### Option 2: Manual Setup

If you prefer to run services individually:

1. **Start the database**:
   ```bash
   cd infrastructure
   docker-compose up db
   ```

2. **Start the backend**:
   ```bash
   cd backend
   dotnet run
   ```

3. **Start the frontend**:
   ```bash
   cd frontend
   npm install
   npm start
   ```

## Project Structure

```
├── backend/            # .NET Core API
├── frontend/          # React TypeScript app
├── infrastructure/    # Docker configuration
└── memory-bank/       # Project documentation
```

## Features

- **Real-time Chat**: Direct and group messaging
- **User Authentication**: Secure login/register
- **Contact Management**: Add and manage contacts
- **Group Management**: Create and manage group chats
- **Responsive Design**: Works on desktop and mobile

## Tech Stack

- **Backend**: .NET Core 8, Entity Framework, PostgreSQL
- **Frontend**: React 18, TypeScript, Tailwind CSS
- **Database**: PostgreSQL 16
- **Infrastructure**: Docker, Docker Compose

## Development

### Prerequisites

- Docker and Docker Compose
- Node.js 18+ (for local frontend development)
- .NET 8 SDK (for local backend development)

### Running in Development Mode

The Docker Compose setup includes volume mounts for hot reloading:

- Frontend code changes will automatically refresh the browser
- Backend changes require rebuilding the container

### Database Access

The PostgreSQL database is accessible at `localhost:5432` with:
- **Database**: mychat
- **Username**: postgres
- **Password**: postgres

### Environment Variables

The application uses the following key environment variables:

- `REACT_APP_API_URL`: Frontend API URL (default: http://localhost:8080)
- `ConnectionStrings__DefaultConnection`: Database connection string
- `ASPNETCORE_ENVIRONMENT`: Backend environment (Development/Production)

## Troubleshooting

1. **Port conflicts**: Ensure ports 3000, 8080, and 5432 are available
2. **Docker issues**: Try `docker-compose down -v` to reset volumes
3. **Database connection**: Wait for the database health check to pass

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License.

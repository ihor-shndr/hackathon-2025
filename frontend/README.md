# MyChat Frontend

A modern React TypeScript frontend for the MyChat application with a clean, responsive design built using Tailwind CSS.

## Features

- **Authentication**: Login and registration with form validation
- **Real-time Chat Interface**: Clean chat UI with message history
- **Contact Management**: Search users, send invitations, manage contacts
- **Group Management**: Create and manage group chats
- **Responsive Design**: Works on desktop and mobile devices
- **TypeScript**: Full TypeScript support for type safety

## Tech Stack

- **React 18** with TypeScript
- **Tailwind CSS** for styling
- **React Router** for navigation
- **Axios** for API communication
- **Context API** for state management

## Prerequisites

- Node.js 16+ and npm
- Backend API running on `http://localhost:8080`

## Getting Started

1. **Install dependencies**:
   ```bash
   npm install
   ```

2. **Start the development server**:
   ```bash
   npm start
   ```

3. **Open your browser**:
   Navigate to `http://localhost:3000`

## Available Scripts

- `npm start` - Runs the app in development mode
- `npm run build` - Builds the app for production
- `npm test` - Launches the test runner
- `npm run eject` - Ejects from Create React App (one-way operation)

## Project Structure

```
src/
├── components/          # Reusable components
│   ├── chat/           # Chat-specific components
│   └── ProtectedRoute.tsx
├── contexts/           # React contexts
│   └── AuthContext.tsx
├── pages/              # Page components
│   ├── Login.tsx
│   ├── Register.tsx
│   └── Chat.tsx
├── services/           # API services
│   └── api.ts
├── types/              # TypeScript type definitions
│   └── index.ts
└── App.tsx             # Main app component
```

## API Integration

The frontend communicates with the backend API using the following endpoints:

- `POST /api/auth/login` - User authentication
- `POST /api/auth/register` - User registration
- `GET /api/contacts` - Get user contacts
- `POST /api/contacts/invite` - Send contact invitation
- `GET /api/groups` - Get user groups
- `POST /api/messages/direct` - Send direct message
- `POST /api/messages/group` - Send group message
- `GET /api/messages/conversations` - Get conversations

## Key Features

### Authentication
- Secure login/register forms with validation
- Token-based authentication with auto-refresh
- Protected routes for authenticated users only

### Chat Interface
- Real-time message display with timestamps
- Conversation list with search functionality
- Support for both direct and group messages
- Message input with send functionality

### Contact Management
- Search and invite new users
- Manage contact invitations (accept/decline)
- View online status of contacts

### Group Management
- Create new groups with descriptions
- Manage group membership
- Group chat functionality

## Development Notes

- The app uses a proxy configuration to forward API calls to `localhost:8080`
- TypeScript strict mode is enabled for better type safety
- Tailwind CSS is configured with custom scrollbar styles
- The app is fully responsive and mobile-friendly

## Building for Production

1. **Build the application**:
   ```bash
   npm run build
   ```

2. **The build folder** will contain the production-ready files that can be served by any static file server.

## Environment Configuration

The frontend is configured to work with the backend running on `http://localhost:8080`. If your backend runs on a different port, update the `baseURL` in `src/services/api.ts`.

## Troubleshooting

1. **API calls failing**: Ensure the backend is running on `http://localhost:8080`
2. **TypeScript errors**: Run `npm install` to ensure all type definitions are installed
3. **Tailwind styles not loading**: Ensure the build process completed successfully

## Future Enhancements

- WebSocket integration for real-time messaging
- File upload and sharing
- Push notifications
- Dark mode support
- Mobile app using React Native

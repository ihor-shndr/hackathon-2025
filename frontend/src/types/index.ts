// User types
export interface User {
  id: number;
  username: string;
  email?: string;
  createdAt?: string;
}

// Authentication types
export interface LoginCredentials {
  username: string;
  password: string;
}

export interface RegisterData {
  username: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponseData {
  token: string;
  user: User;
}

// Authentication response data (what's inside ApiResponse<T>.data)
export interface AuthResponse {
  token: string;
  user: User;
}

// Contact types
export interface Contact {
  id: number;
  user: {
    id: number;
    username: string;
    createdAt: string;
  };
  status: ContactStatus;
  createdAt: string;
  acceptedAt?: string;
  message?: string;
}

export enum ContactStatus {
  Pending = 'pending',
  Accepted = 'accepted',
  Rejected = 'rejected',
  Blocked = 'blocked'
}

export interface ContactInvitation {
  id: number;
  fromUser: User;
  toUser: User;
  message: string;
  status: 'pending' | 'accepted' | 'rejected';
  createdAt: string;
}

export interface InvitationData {
  username: string;
  message: string;
}

// Group types
export interface Group {
  id: number;
  name: string;
  description?: string;
  ownerId: number;
  memberCount: number;
  createdAt: string;
}

export interface GroupMember {
  id: number;
  username: string;
  role: 'owner' | 'admin' | 'member';
  joinedAt: string;
}

export interface CreateGroupData {
  name: string;
  description?: string;
  initialMemberIds?: number[];
}

// Message types - Updated to use string enums instead of magic numbers
export type MessageType = 'Text' | 'Image' | 'File' | 'Audio' | 'Video';

// Conversation types - Updated to use string enums instead of magic numbers
export type ConversationType = 'direct' | 'group';

export interface Message {
  id: number;
  sender: {
    id: number;
    username: string;
    createdAt?: string;
  };
  content: string;
  type: MessageType; // Updated to use string enum
  attachmentUrl?: string;
  sentAt: string; // Backend uses 'sentAt', not 'timestamp'
  isOwn: boolean;
  groupId?: number;
  groupName?: string;
  recipientId?: number;
  recipientUsername?: string;
}

export interface SendMessageData {
  content: string;
  type: MessageType; // Updated to use string enum
  attachmentUrl?: string;
  recipientId?: number;
  groupId?: number;
}

// Updated to match backend ConversationDto with string enums
export interface Conversation {
  id: number;
  type: ConversationType; // Updated to use string enum: 'direct' | 'group'
  name: string;
  lastMessage?: Message;
  unreadCount: number;
  lastActivity: string;
  
  // For direct messages
  contactId?: number;
  
  // For groups
  groupId?: number;
  memberCount?: number;
}

// API Response types - Updated to match C# backend format
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
}

// Error types
export interface ApiError {
  message: string;
  details?: any;
}

// UI State types
export interface LoadingState {
  isLoading: boolean;
  error?: string;
}

export interface ChatState {
  activeConversation?: Conversation;
  messages: Message[];
  contacts: Contact[];
  groups: Group[];
  invitations: ContactInvitation[];
} 
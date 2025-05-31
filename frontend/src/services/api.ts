import axios, { AxiosResponse } from 'axios';
import { 
  LoginCredentials, 
  RegisterData, 
  AuthResponseData,
  ApiResponse,
  Contact,
  ContactInvitation,
  InvitationData,
  Group,
  GroupMember,
  CreateGroupData,
  Message,
  SendMessageData,
  Conversation,
  PaginatedResponse,
  User
} from '../types';

// Get API base URL from environment variable or fallback to localhost
const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || 'http://localhost:8080';

// Create axios instance with base configuration
const api = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  timeout: parseInt(process.env.REACT_APP_API_TIMEOUT || '10000'),
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor to include auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Add response interceptor for error handling and data extraction
api.interceptors.response.use(
  (response) => {
    // Extract data from ApiResponse<T> format for successful responses
    if (response.data && typeof response.data === 'object' && 'success' in response.data) {
      // This is an ApiResponse<T>, extract the actual data
      return {
        ...response,
        data: response.data.data // Extract T from ApiResponse<T>
      };
    }
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth API
export const authAPI = {
  register: (userData: RegisterData): Promise<AxiosResponse<void>> => 
    api.post('/auth/register', userData),
  login: (credentials: LoginCredentials): Promise<AxiosResponse<AuthResponseData>> => 
    api.post('/auth/login', credentials),
  checkUsername: (username: string): Promise<AxiosResponse<{ available: boolean }>> => 
    api.get(`/auth/check-username/${username}`),
};

// Contacts API
export const contactsAPI = {
  getContacts: (): Promise<AxiosResponse<Contact[]>> => 
    api.get('/contacts'),
  searchUsers: (query: string): Promise<AxiosResponse<User[]>> => 
    api.get(`/contacts/search?query=${query}`),
  sendInvitation: (data: InvitationData): Promise<AxiosResponse<void>> => 
    api.post('/contacts/invite', data),
  getInvitations: (): Promise<AxiosResponse<ContactInvitation[]>> => 
    api.get('/contacts/invitations'),
  getSentInvitations: (): Promise<AxiosResponse<ContactInvitation[]>> => 
    api.get('/contacts/invitations/sent'),
  respondToInvitation: (id: number, accept: boolean): Promise<AxiosResponse<void>> => 
    api.put(`/contacts/invitation/${id}/respond`, { accept }),
  blockContact: (id: number): Promise<AxiosResponse<void>> => 
    api.put(`/contacts/${id}/block`),
  removeContact: (id: number): Promise<AxiosResponse<void>> => 
    api.delete(`/contacts/${id}`),
};

// Groups API
export const groupsAPI = {
  getGroups: (): Promise<AxiosResponse<Group[]>> => 
    api.get('/groups'),
  getGroup: (id: number): Promise<AxiosResponse<Group>> => 
    api.get(`/groups/${id}`),
  createGroup: (data: CreateGroupData): Promise<AxiosResponse<Group>> => 
    api.post('/groups', data),
  updateGroup: (id: number, data: Partial<CreateGroupData>): Promise<AxiosResponse<Group>> => 
    api.put(`/groups/${id}`, data),
  deleteGroup: (id: number): Promise<AxiosResponse<void>> => 
    api.delete(`/groups/${id}`),
  getMembers: (id: number): Promise<AxiosResponse<GroupMember[]>> => 
    api.get(`/groups/${id}/members`),
  addMember: (groupId: number, memberId: number): Promise<AxiosResponse<void>> => 
    api.post(`/groups/${groupId}/members/${memberId}`),
  removeMember: (groupId: number, memberId: number): Promise<AxiosResponse<void>> => 
    api.delete(`/groups/${groupId}/members/${memberId}`),
  leaveGroup: (id: number): Promise<AxiosResponse<void>> => 
    api.post(`/groups/${id}/leave`),
};

// Messages API
export const messagesAPI = {
  sendDirectMessage: (data: SendMessageData): Promise<AxiosResponse<Message>> => 
    api.post('/messages/direct', data),
  sendGroupMessage: (data: SendMessageData): Promise<AxiosResponse<Message>> => 
    api.post('/messages/group', data),
  getDirectMessages: (userId: number, page = 1, pageSize = 50): Promise<AxiosResponse<PaginatedResponse<Message>>> => 
    api.get(`/messages/direct/${userId}?page=${page}&pageSize=${pageSize}`),
  getGroupMessages: (groupId: number, page = 1, pageSize = 50): Promise<AxiosResponse<PaginatedResponse<Message>>> => 
    api.get(`/messages/group/${groupId}?page=${page}&pageSize=${pageSize}`),
  getConversations: (): Promise<AxiosResponse<Conversation[]>> => 
    api.get('/messages/conversations'),
  searchMessages: (query: string): Promise<AxiosResponse<Message[]>> => 
    api.get(`/messages/search?query=${query}`),
  deleteMessage: (id: number): Promise<AxiosResponse<void>> => 
    api.delete(`/messages/${id}`),
};

// Images API
export const imagesAPI = {
  uploadImage: (file: File): Promise<AxiosResponse<string>> => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post('/images/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },
};

export default api;

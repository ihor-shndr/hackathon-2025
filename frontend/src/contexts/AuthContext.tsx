import React, { createContext, useState, useContext, useEffect, ReactNode } from 'react';
import { authAPI } from '../services/api';
import { User, LoginCredentials, RegisterData } from '../types';

interface AuthContextType {
  user: User | null;
  token: string | null;
  loading: boolean;
  login: (credentials: LoginCredentials) => Promise<{ success: boolean; error?: string }>;
  register: (userData: RegisterData) => Promise<{ success: boolean; error?: string }>;
  logout: () => void;
  checkUsername: (username: string) => Promise<{ available: boolean }>;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [token, setToken] = useState<string | null>(localStorage.getItem('accessToken'));

  useEffect(() => {
    // Check if user is logged in on app start
    const storedToken = localStorage.getItem('accessToken');
    if (storedToken) {
      setToken(storedToken);
      // In a real app, you might want to validate the token with the backend
      // For now, we'll assume the token is valid if it exists
      const username = localStorage.getItem('username');
      if (username) {
        setUser({ id: 0, username }); // We'll get proper user data from API later
      }
    }
    setLoading(false);
  }, []);

  const login = async (credentials: LoginCredentials): Promise<{ success: boolean; error?: string }> => {
    try {
      const response = await authAPI.login(credentials);
      console.log('Login response (after extraction):', response.data); // Debug log
      
      // The API interceptor has already extracted the data from ApiResponse<AuthResponseData>
      // So response.data is now directly the AuthResponseData object
      const authData = response.data;
      
      if (authData && authData.token) {
        const newToken = authData.token;
        const userData = authData.user || { id: 0, username: credentials.username };
        
        console.log('Extracted token:', newToken); // Debug log
        
        localStorage.setItem('accessToken', newToken);
        localStorage.setItem('username', userData.username);
        
        setToken(newToken);
        setUser(userData);
        
        return { success: true };
      } else {
        throw new Error('No valid token found in login response. Response: ' + JSON.stringify(authData));
      }
    } catch (error: any) {
      console.error('Login error:', error);
      
      // Handle API response errors (ApiResponse<T> format)
      let errorMessage = 'Login failed';
      if (error.response?.data) {
        if (error.response.data.message) {
          // ApiResponse format error
          errorMessage = error.response.data.message;
        } else if (error.response.data.errors && error.response.data.errors.length > 0) {
          // Multiple errors from ApiResponse
          errorMessage = error.response.data.errors.join(', ');
        }
      } else if (error.message) {
        errorMessage = error.message;
      }
      
      return { 
        success: false, 
        error: errorMessage
      };
    }
  };

  const register = async (userData: RegisterData): Promise<{ success: boolean; error?: string }> => {
    try {
      await authAPI.register(userData);
      return { success: true };
    } catch (error: any) {
      console.error('Registration error:', error);
      
      // Handle API response errors (ApiResponse<T> format)
      let errorMessage = 'Registration failed';
      if (error.response?.data) {
        if (error.response.data.message) {
          // ApiResponse format error
          errorMessage = error.response.data.message;
        } else if (error.response.data.errors && error.response.data.errors.length > 0) {
          // Multiple errors from ApiResponse
          errorMessage = error.response.data.errors.join(', ');
        }
      } else if (error.message) {
        errorMessage = error.message;
      }
      
      return { 
        success: false, 
        error: errorMessage
      };
    }
  };

  const logout = (): void => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('username');
    setToken(null);
    setUser(null);
  };

  const checkUsername = async (username: string): Promise<{ available: boolean }> => {
    try {
      const response = await authAPI.checkUsername(username);
      return response.data;
    } catch (error: any) {
      console.error('Username check error:', error);
      return { available: false };
    }
  };

  const value: AuthContextType = {
    user,
    token,
    loading,
    login,
    register,
    logout,
    checkUsername,
    isAuthenticated: !!token,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}; 
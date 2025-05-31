import React, { useState, useRef, useEffect } from 'react';
import { Conversation, Message } from '../../types';
import { useAuth } from '../../contexts/AuthContext';
import ImageUpload from './ImageUpload';

interface ChatAreaProps {
  conversation: Conversation | null;
  messages: Message[];
  onSendMessage: (content: string) => void;
}

const ChatArea: React.FC<ChatAreaProps> = ({
  conversation,
  messages,
  onSendMessage
}) => {
  const { user } = useAuth();
  const [newMessage, setNewMessage] = useState('');
  const [error, setError] = useState<string | null>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (newMessage.trim()) {
      onSendMessage(newMessage);
      setNewMessage('');
      setError(null);
    }
  };

  const handleImageUploaded = (imageUrl: string) => {
    onSendMessage(imageUrl);
    setError(null);
  };

  const handleImageError = (errorMessage: string) => {
    setError(errorMessage);
    setTimeout(() => setError(null), 5000);
  };

  const isImageUrl = (content: string): boolean => {
    // Check for image file extensions
    const hasImageExtension = /\.(jpg|jpeg|png|gif|webp)$/i.test(content);
    // Check for our API image URLs (they start with /api/images/)
    const isApiImageUrl = content.startsWith('/api/images/');
    // Check for old S3 URLs (for backward compatibility)
    const isS3Url = content.includes('amazonaws.com');
    
    return hasImageExtension || isApiImageUrl || isS3Url;
  };

  const formatMessageTime = (sentAt: string) => {
    const date = new Date(sentAt);
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit',
      hour12: false 
    });
  };

  const formatDateSeparator = (sentAt: string) => {
    const date = new Date(sentAt);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    if (date.toDateString() === today.toDateString()) {
      return 'Today';
    } else if (date.toDateString() === yesterday.toDateString()) {
      return 'Yesterday';
    } else {
      return date.toLocaleDateString('en-US', { 
        weekday: 'long', 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
      });
    }
  };

  const shouldShowDateSeparator = (currentMessage: Message, previousMessage?: Message) => {
    if (!previousMessage) return true;
    
    const currentDate = new Date(currentMessage.sentAt).toDateString();
    const previousDate = new Date(previousMessage.sentAt).toDateString();
    
    return currentDate !== previousDate;
  };

  if (!conversation) {
    return (
      <div className="flex-1 flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <svg className="mx-auto h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
          <h3 className="mt-4 text-lg font-medium text-gray-900">Select a conversation</h3>
          <p className="mt-2 text-sm text-gray-500">Choose a conversation from the sidebar to start messaging</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full">
      {/* Chat Header */}
      <div className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-lg font-semibold text-gray-900">{conversation.name}</h2>
            <p className="text-sm text-gray-500">
              {conversation.type === 'Group'
                ? `Group chat • ${conversation.memberCount || 0} members`
                : 'Direct message'
              }
            </p>
          </div>
          <div className="flex items-center space-x-2">
            {conversation.type === 'Group' && (
              <button className="p-2 rounded-lg hover:bg-gray-100 transition-colors">
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
              </button>
            )}
            <button className="p-2 rounded-lg hover:bg-gray-100 transition-colors">
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </button>
          </div>
        </div>
      </div>

      {/* Messages Area */}
      <div className="flex-1 overflow-y-auto p-6 space-y-4 scrollbar-thin">
        {messages.length === 0 ? (
          <div className="text-center text-gray-500 mt-8">
            <p>No messages yet. Start the conversation!</p>
          </div>
        ) : (
          messages.map((message, index) => {
            const previousMessage = index > 0 ? messages[index - 1] : undefined;
            const isCurrentUser = message.isOwn || message.sender.id === user?.id;
            const showDateSeparator = shouldShowDateSeparator(message, previousMessage);

            return (
              <React.Fragment key={message.id}>
                {showDateSeparator && (
                  <div className="text-center my-4">
                    <span className="bg-gray-100 text-gray-600 text-xs px-3 py-1 rounded-full">
                      {formatDateSeparator(message.sentAt)}
                    </span>
                  </div>
                )}
                
                <div className={`flex ${isCurrentUser ? 'justify-end' : 'justify-start'} mb-3`}>
                  <div className={`${
                    isImageUrl(message.content) 
                      ? 'max-w-sm lg:max-w-lg' 
                      : 'max-w-xs lg:max-w-md'
                  } px-4 py-3 rounded-lg ${
                    isCurrentUser 
                      ? 'bg-blue-600 text-white' 
                      : 'bg-gray-200 text-gray-900'
                  }`}>
                    {!isCurrentUser && conversation.type === 'Group' && (
                      <p className="text-xs font-medium mb-2 opacity-75">
                        {message.sender.username}
                      </p>
                    )}
                    {isImageUrl(message.content) ? (
                      <div className="w-full">
                        <img 
                          src={message.content.startsWith('/api/') 
                            ? `${process.env.REACT_APP_API_URL || 'http://localhost:8080'}${message.content}`
                            : message.content
                          } 
                          alt="Shared image" 
                          className="rounded-lg w-full h-auto max-w-full object-contain"
                          style={{ maxHeight: '300px', maxWidth: '100%' }}
                          onError={(e) => {
                            const target = e.target as HTMLImageElement;
                            target.style.display = 'none';
                            const fallback = target.nextElementSibling as HTMLElement;
                            if (fallback) fallback.style.display = 'block';
                          }}
                        />
                        <p className="text-sm leading-relaxed" style={{ display: 'none' }}>
                          {message.content}
                        </p>
                      </div>
                    ) : (
                      <p className="text-sm leading-relaxed">{message.content}</p>
                    )}
                    <p className={`text-xs mt-2 ${
                      isCurrentUser ? 'text-blue-100' : 'text-gray-500'
                    }`}>
                      {formatMessageTime(message.sentAt)}
                    </p>
                  </div>
                </div>
              </React.Fragment>
            );
          })
        )}
        <div ref={messagesEndRef} />
      </div>

      {/* Message Input */}
      <div className="bg-white border-t border-gray-200 px-6 py-4">
        {error && (
          <div className="mb-3 p-3 bg-red-100 border border-red-400 text-red-700 rounded-lg text-sm">
            {error}
          </div>
        )}
        <div className="flex items-center" style={{ gap: '12px' }}>
          <ImageUpload 
            onImageUploaded={handleImageUploaded}
            onError={handleImageError}
            disabled={false}
          />
          <input
            type="text"
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            placeholder={`Message ${conversation.name}...`}
            className="flex-1 px-4 py-3 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            onKeyDown={(e) => {
              if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                handleSubmit(e);
              }
            }}
          />
          <button
            type="button"
            onClick={handleSubmit}
            disabled={!newMessage.trim()}
            className="px-6 py-3 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            style={{ minWidth: '80px' }}
          >
            Send
          </button>
        </div>
      </div>
    </div>
  );
};

export default ChatArea;

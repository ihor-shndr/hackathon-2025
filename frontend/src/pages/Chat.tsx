import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';
import Sidebar from '../components/chat/Sidebar';
import ChatArea from '../components/chat/ChatArea';
import ContactPanel from '../components/chat/ContactPanel';
import { Conversation, Message, Contact, Group } from '../types';
import { messagesAPI, contactsAPI, groupsAPI } from '../services/api';
import { getSignalRService, disposeSignalRService } from '../services/signalr';

const Chat: React.FC = () => {
  const { user, logout } = useAuth();
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activeConversation, setActiveConversation] = useState<Conversation | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [groups, setGroups] = useState<Group[]>([]);
  const [loading, setLoading] = useState(true);
  const [showContactPanel, setShowContactPanel] = useState(false);
  const [signalRConnected, setSignalRConnected] = useState(false);
  const signalRService = useRef(getSignalRService());

  // Move loadConversations to useCallback to avoid recreating it
  const loadConversations = useCallback(async () => {
    try {
      const conversationsRes = await messagesAPI.getConversations();
      setConversations(conversationsRes.data);
    } catch (error) {
      console.error('Failed to load conversations:', error);
    }
  }, []);

  // Create event handlers that always have access to current state
  const handleDirectMessage = useCallback((message: Message) => {
    console.log('Received real-time direct message:', message);
    console.log('Message details:', {
      messageId: message.id,
      senderId: message.sender.id,
      recipientId: message.recipientId,
      currentUserId: user?.id,
      activeConversationContactId: activeConversation?.contactId
    });
    
    // Add message to current conversation if it matches
    setMessages(prev => {
      // Check if this message belongs to the currently active conversation
      const currentActiveConversation = activeConversation;
      if (currentActiveConversation && currentActiveConversation.type === 'Direct') {
        // For direct messages, check if either:
        // 1. I sent the message to the contact I'm chatting with
        // 2. The contact I'm chatting with sent the message to me
        const isFromActiveContact = message.sender.id === currentActiveConversation.contactId;
        const isToActiveContact = message.recipientId === currentActiveConversation.contactId && message.sender.id === user?.id;
        
        if (isFromActiveContact || isToActiveContact) {
          // Avoid duplicates
          if (prev.some(m => m.id === message.id)) return prev;
          return [...prev, message];
        }
      }
      return prev;
    });
    
    // Always refresh conversations to update last message
    loadConversations();
  }, [activeConversation, user?.id, loadConversations]);

  const handleGroupMessage = useCallback((message: Message) => {
    console.log('Received real-time group message:', message);
    console.log('Group message details:', {
      messageId: message.id,
      senderId: message.sender.id,
      groupId: message.groupId,
      activeConversationGroupId: activeConversation?.groupId,
      activeConversationType: activeConversation?.type
    });
    
    // Add message to current conversation if it matches
    setMessages(prev => {
      const currentActiveConversation = activeConversation;
      if (currentActiveConversation && 
          currentActiveConversation.type === 'Group' && 
          message.groupId === currentActiveConversation.groupId) {
        // Avoid duplicates
        if (prev.some(m => m.id === message.id)) return prev;
        console.log('Adding group message to current conversation');
        return [...prev, message];
      }
      console.log('Group message does not match current conversation');
      return prev;
    });
    
    // Always refresh conversations to update last message
    loadConversations();
  }, [activeConversation, loadConversations]);

  const handleConnectionStateChange = useCallback((connected: boolean) => {
    console.log('SignalR connection state changed:', connected);
    setSignalRConnected(connected);
    
    // If we just connected and have an active group conversation, join the group
    if (connected && activeConversation?.type === 'Group' && activeConversation.groupId) {
      const service = signalRService.current;
      service.joinGroup(activeConversation.groupId).then(() => {
        console.log(`Auto-joined SignalR group ${activeConversation.groupId} after connection`);
      }).catch(error => {
        console.error('Failed to auto-join group after connection:', error);
      });
    }
  }, [activeConversation]);

  // Set up SignalR event handlers whenever the callbacks change
  useEffect(() => {
    const service = signalRService.current;
    
    // Set up event handlers with current callbacks
    service.onDirectMessageReceived(handleDirectMessage);
    service.onGroupMessageReceived(handleGroupMessage);
    service.onConnectionStateChanged(handleConnectionStateChange);
    
    console.log('SignalR event handlers updated');
  }, [handleDirectMessage, handleGroupMessage, handleConnectionStateChange]);

  // Initialize SignalR connection once
  useEffect(() => {
    const initializeSignalR = async () => {
      const service = signalRService.current;
      console.log('Initializing SignalR connection...');
      await service.connect();
    };

    loadInitialData();
    initializeSignalR();
    
    // Cleanup on unmount
    return () => {
      disposeSignalRService();
    };
  }, []);

  const loadInitialData = async () => {
    try {
      const [conversationsRes, contactsRes, groupsRes] = await Promise.all([
        messagesAPI.getConversations(),
        contactsAPI.getContacts(),
        groupsAPI.getGroups(),
      ]);

      setConversations(conversationsRes.data);
      setContacts(contactsRes.data);
      setGroups(groupsRes.data);
    } catch (error) {
      console.error('Failed to load initial data:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadMessages = async (conversation: Conversation) => {
    try {
      let response;
      // Handle backend string enum types: 'direct' and 'group'
      if (conversation.type === 'Direct' && conversation.contactId) {
        response = await messagesAPI.getDirectMessages(conversation.contactId);
      } else if (conversation.type === 'Group' && conversation.groupId) {
        response = await messagesAPI.getGroupMessages(conversation.groupId);
      }
      
      if (response) {
        // Handle paginated response - the data is in response.data.data (array of messages)
        const messagesData = Array.isArray(response.data) ? response.data : response.data.data || [];
        setMessages(messagesData);
      }
    } catch (error) {
      console.error('Failed to load messages:', error);
      setMessages([]); // Clear messages on error
    }
  };

  const handleConversationSelect = async (conversation: Conversation) => {
    // Leave current group if we're switching from a group conversation
    if (activeConversation?.type === 'Group' && activeConversation.groupId) {
      const service = signalRService.current;
      if (service.isConnected()) {
        await service.leaveGroup(activeConversation.groupId);
        console.log(`Left SignalR group ${activeConversation.groupId}`);
      }
    }

    setActiveConversation(conversation);
    loadMessages(conversation);

    // Join new group if we're switching to a group conversation
    if (conversation.type === 'Group' && conversation.groupId) {
      const service = signalRService.current;
      if (service.isConnected()) {
        await service.joinGroup(conversation.groupId);
        console.log(`Joined SignalR group ${conversation.groupId}`);
      } else {
        console.warn('SignalR not connected, will join group when connection is established');
      }
    }
  };

  const handleSendMessage = async (content: string) => {
    if (!activeConversation || !content.trim()) {
      console.log('Early exit:', { activeConversation, content: content.trim() });
      return;
    }

    console.log('Sending message:', { content, conversation: activeConversation });
    console.log('Conversation details:', {
      id: activeConversation.id,
      type: activeConversation.type,
      name: activeConversation.name,
      contactId: activeConversation.contactId,
      groupId: activeConversation.groupId
    });

    try {
      let response: any = null;
      const messageData = {
        content: content.trim(),
        type: 'Text' as const, // Backend accepts string enum: 'Text'
      };

      console.log('Message data to send:', messageData);

      // Handle backend string enum types: 'direct' and 'group'
      if (activeConversation.type === 'Direct' && activeConversation.contactId) {
        console.log('Sending direct message to:', activeConversation.contactId);
        response = await messagesAPI.sendDirectMessage({
          ...messageData,
          recipientId: activeConversation.contactId,
        });
      } else if (activeConversation.type === 'Group' && activeConversation.groupId) {
        console.log('Sending group message to group:', activeConversation.groupId);
        console.log('Full group message payload:', {
          ...messageData,
          groupId: activeConversation.groupId,
        });
        response = await messagesAPI.sendGroupMessage({
          ...messageData,
          groupId: activeConversation.groupId,
        });
      } else {
        console.error('Invalid conversation state:', {
          type: activeConversation.type,
          typeOf: typeof activeConversation.type,
          contactId: activeConversation.contactId,
          groupId: activeConversation.groupId,
          'type === direct': activeConversation.type === 'Direct',
          'type === group': activeConversation.type === 'Group',
          'has contactId': !!activeConversation.contactId,
          'has groupId': !!activeConversation.groupId
        });
        return;
      }

      console.log('Message send response:', response);

      if (response && response.data) {
        // Immediately add the sent message to UI for instant feedback
        const sentMessage: Message = response.data;
        console.log('Adding sent message to UI:', sentMessage);
        
        setMessages(prev => {
          // Avoid duplicates - check if message already exists
          if (prev.some(m => m.id === sentMessage.id)) {
            console.log('Message already exists, skipping duplicate');
            return prev;
          }
          console.log('Adding new sent message to conversation');
          return [...prev, sentMessage];
        });
        
        // SignalR will handle broadcasting to other users
        console.log('Message sent successfully and added to UI');
      } else {
        console.error('No response data received');
      }
    } catch (error: any) {
      console.error('Failed to send message:', error);
      console.error('Error details:', {
        message: error.message,
        response: error.response?.data,
        status: error.response?.status
      });
      
      // Show user-friendly error
      alert(`Failed to send message: ${error.response?.data?.message || error.message || 'Unknown error'}`);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  return (
    <div className="h-screen flex bg-gray-100">
      {/* Sidebar - Fixed Width */}
      <div className="bg-white border-r border-gray-200 flex flex-col" style={{ width: '384px', minWidth: '384px', maxWidth: '384px' }}>
        <div className="px-6 py-4 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <h1 className="text-xl font-semibold text-gray-800">MyChat</h1>
            <div className="flex items-center" style={{ gap: '12px' }}>
              <span className="text-sm text-gray-600 px-3 py-2 bg-gray-50 rounded-lg font-medium whitespace-nowrap">
                Welcome, {user?.username}
              </span>
              
              {/* Contacts button */}
              <button
                onClick={() => setShowContactPanel(!showContactPanel)}
                className={`p-2.5 rounded-lg transition-all duration-200 flex items-center space-x-1.5 ${
                  showContactPanel 
                    ? 'bg-blue-100 text-blue-700 hover:bg-blue-200' 
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-700'
                }`}
                title="Manage Contacts"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.196-2.121M17 20H7m10 0v-2c0-5-3-7-7-7s-7 2-7 7v2m10 0H7m10 0H7M9 7a4 4 0 118 0 4 4 0 01-8 0z" />
                </svg>
                <span className="text-xs font-medium">Contacts</span>
              </button>
              
              {/* Logout button */}
              <button
                onClick={logout}
                className="p-2.5 rounded-lg hover:bg-red-50 transition-all duration-200 text-gray-500 hover:text-red-600 flex items-center"
                title="Logout"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
              </button>
            </div>
          </div>
        </div>
        <Sidebar
          conversations={conversations}
          activeConversation={activeConversation}
          onConversationSelect={handleConversationSelect}
          groups={groups}
          contacts={contacts}
          onStartGroupChat={(group) => {
            // Create a group conversation and load it
            const groupConversation: Conversation = {
              id: group.id,
              type: 'Group', // Backend enum: 'Group'
              name: group.name,
              unreadCount: 0,
              lastActivity: new Date().toISOString(),
              groupId: group.id,
              memberCount: group.memberCount
            };
            
            // Use the same handler to ensure SignalR group joining
            handleConversationSelect(groupConversation);
          }}
          onStartDirectChat={(contact) => {
            // Create a direct conversation and load it
            const directConversation: Conversation = {
              id: contact.user.id, // Use the actual user ID, not contact record ID
              type: 'Direct', // Backend enum: 'Direct'
              name: contact.user.username,
              unreadCount: 0,
              lastActivity: new Date().toISOString(),
              contactId: contact.user.id // This should be the user ID for API calls
            };
            
            // Use the same handler to ensure proper message loading
            handleConversationSelect(directConversation);
          }}
        />
      </div>

      {/* Main Chat Area - Flexible Width */}
      <div className="flex flex-col" style={{ flex: showContactPanel ? '1 1 0' : '1 1 auto', minWidth: '400px' }}>
        <ChatArea
          conversation={activeConversation}
          messages={messages}
          onSendMessage={handleSendMessage}
        />
      </div>

      {/* Contact Panel - Fixed Width, Always Reserve Space */}
      <div className="bg-white border-l border-gray-200 transition-all duration-200" style={{ 
        width: showContactPanel ? '384px' : '0px',
        minWidth: showContactPanel ? '384px' : '0px',
        maxWidth: showContactPanel ? '384px' : '0px',
        overflow: showContactPanel ? 'visible' : 'hidden'
      }}>
        {showContactPanel && (
          <ContactPanel
            contacts={contacts}
            groups={groups}
            onClose={() => setShowContactPanel(false)}
            onDataUpdate={loadInitialData}
            onStartDirectChat={(contact) => {
              // Create a direct conversation and load it
              const directConversation: Conversation = {
                id: contact.user.id, // Use the actual user ID, not contact record ID
                type: 'Direct', // Backend enum: 'Direct'
                name: contact.user.username,
                unreadCount: 0,
                lastActivity: new Date().toISOString(),
                contactId: contact.user.id // This should be the user ID for API calls
              };
              
              // Use the same handler to ensure proper message loading
              handleConversationSelect(directConversation);
            }}
            onStartGroupChat={(group) => {
              // Create a group conversation and load it
              const groupConversation: Conversation = {
                id: group.id,
                type: 'Group', // Backend enum: 'Group'
                name: group.name,
                unreadCount: 0,
                lastActivity: new Date().toISOString(),
                groupId: group.id,
                memberCount: group.memberCount
              };
              
              // Use the same handler to ensure SignalR group joining
              handleConversationSelect(groupConversation);
            }}
          />
        )}
      </div>
    </div>
  );
};

export default Chat;

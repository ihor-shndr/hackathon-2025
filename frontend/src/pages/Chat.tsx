import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import Sidebar from '../components/chat/Sidebar';
import ChatArea from '../components/chat/ChatArea';
import ContactPanel from '../components/chat/ContactPanel';
import { Conversation, Message, Contact, Group } from '../types';
import { messagesAPI, contactsAPI, groupsAPI } from '../services/api';

const Chat: React.FC = () => {
  const { user, logout } = useAuth();
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activeConversation, setActiveConversation] = useState<Conversation | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [groups, setGroups] = useState<Group[]>([]);
  const [loading, setLoading] = useState(true);
  const [showContactPanel, setShowContactPanel] = useState(false);

  useEffect(() => {
    loadInitialData();
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
      if (conversation.type === 'direct' && conversation.contactId) {
        response = await messagesAPI.getDirectMessages(conversation.contactId);
      } else if (conversation.type === 'group' && conversation.groupId) {
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

  const handleConversationSelect = (conversation: Conversation) => {
    setActiveConversation(conversation);
    loadMessages(conversation);
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
      if (activeConversation.type === 'direct' && activeConversation.contactId) {
        console.log('Sending direct message to:', activeConversation.contactId);
        response = await messagesAPI.sendDirectMessage({
          ...messageData,
          recipientId: activeConversation.contactId,
        });
      } else if (activeConversation.type === 'group' && activeConversation.groupId) {
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
          'type === direct': activeConversation.type === 'direct',
          'type === group': activeConversation.type === 'group',
          'has contactId': !!activeConversation.contactId,
          'has groupId': !!activeConversation.groupId
        });
        return;
      }

      console.log('Message send response:', response);

      if (response && response.data) {
        // Add the new message to the current messages
        setMessages(prev => [...prev, response.data]);
        
        // Refresh conversations to update last message
        try {
          const conversationsRes = await messagesAPI.getConversations();
          setConversations(conversationsRes.data);
        } catch (convError) {
          console.error('Failed to refresh conversations:', convError);
          // Don't fail the whole operation if conversation refresh fails
        }
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
      {/* Sidebar */}
      <div className="w-96 bg-white border-r border-gray-200 flex flex-col">
        <div className="p-4 border-b border-gray-200 flex items-center justify-between">
          <h1 className="text-xl font-semibold text-gray-800">MyChat</h1>
          <div className="flex items-center space-x-2">
            <span className="text-sm text-gray-600">Welcome, {user?.username}</span>
            
            {/* Contacts button */}
            <button
              onClick={() => setShowContactPanel(!showContactPanel)}
              className="p-2 rounded-lg hover:bg-gray-100 transition-colors flex items-center space-x-1"
              title="Manage Contacts"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.196-2.121M17 20H7m10 0v-2c0-5-3-7-7-7s-7 2-7 7v2m10 0H7m10 0H7M9 7a4 4 0 118 0 4 4 0 01-8 0z" />
              </svg>
              <span className="text-xs">Contacts</span>
            </button>
            
            {/* Logout button */}
            <button
              onClick={logout}
              className="p-2 rounded-lg hover:bg-gray-100 transition-colors text-red-600"
              title="Logout"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
            </button>
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
              type: 'group', // Backend enum: 'group'
              name: group.name,
              unreadCount: 0,
              lastActivity: new Date().toISOString(),
              groupId: group.id,
              memberCount: group.memberCount
            };
            setActiveConversation(groupConversation);
            setMessages([]); // Start with empty messages
          }}
        />
      </div>

      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col">
        <ChatArea
          conversation={activeConversation}
          messages={messages}
          onSendMessage={handleSendMessage}
        />
      </div>

      {/* Contact Panel */}
      {showContactPanel && (
        <div className="w-96 bg-white border-l border-gray-200">
          <ContactPanel
            contacts={contacts}
            groups={groups}
            onClose={() => setShowContactPanel(false)}
            onDataUpdate={loadInitialData}
          />
        </div>
      )}
    </div>
  );
};

export default Chat; 
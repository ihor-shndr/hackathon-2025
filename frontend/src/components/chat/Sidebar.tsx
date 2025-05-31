import React, { useState } from 'react';
import { Conversation, Group, Contact } from '../../types';

interface SidebarProps {
  conversations: Conversation[];
  activeConversation: Conversation | null;
  onConversationSelect: (conversation: Conversation) => void;
  groups?: Group[];
  contacts?: Contact[];
  onStartGroupChat?: (group: Group) => void;
  onStartDirectChat?: (contact: Contact) => void;
}

type SidebarTab = 'all' | 'direct' | 'groups';

const Sidebar: React.FC<SidebarProps> = ({
  conversations,
  activeConversation,
  onConversationSelect,
  groups = [],
  contacts = [],
  onStartGroupChat,
  onStartDirectChat
}) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState<SidebarTab>('all');

  const filteredConversations = conversations.filter(conversation => {
    const matchesSearch = conversation.name.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesTab = activeTab === 'all' || 
      (activeTab === 'direct' && conversation.type === 'Direct') ||
      (activeTab === 'groups' && conversation.type === 'Group');
    return matchesSearch && matchesTab;
  });

  // Find groups that don't have conversations yet
  const availableGroups = groups.filter(group => 
    !conversations.some(conv => conv.type === 'Group' && conv.groupId === group.id)
  );

  // Find contacts that don't have conversations yet
  const availableContacts = contacts.filter(contact => 
    !conversations.some(conv => conv.type === 'Direct' && conv.contactId === contact.user.id)
  );

  const formatTime = (timestamp: string) => {
    const date = new Date(timestamp);
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - date.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays === 1) {
      return date.toLocaleTimeString('en-US', { 
        hour: '2-digit', 
        minute: '2-digit',
        hour12: false 
      });
    } else if (diffDays <= 7) {
      return date.toLocaleDateString('en-US', { weekday: 'short' });
    } else {
      return date.toLocaleDateString('en-US', { 
        month: 'short', 
        day: 'numeric' 
      });
    }
  };

  const handleGroupClick = (group: Group) => {
    // Create a conversation object for the group
    const groupConversation: Conversation = {
      id: group.id,
      type: 'Group', // Backend enum: 'Group' = Group
      name: group.name,
      unreadCount: 0,
      lastActivity: new Date().toISOString(),
      groupId: group.id,
      memberCount: group.memberCount
    };
    onConversationSelect(groupConversation);
    if (onStartGroupChat) {
      onStartGroupChat(group);
    }
  };

  const handleContactClick = (contact: Contact) => {
    // Create a conversation object for the contact
    const directConversation: Conversation = {
      id: contact.user.id, // Use the actual user ID, not contact record ID
      type: 'Direct', // Backend enum: 'Direct' = Direct
      name: contact.user.username,
      unreadCount: 0,
      lastActivity: new Date().toISOString(),
      contactId: contact.user.id // This should be the user ID for API calls
    };
    onConversationSelect(directConversation);
    if (onStartDirectChat) {
      onStartDirectChat(contact);
    }
  };

  return (
    <div className="flex flex-col h-full">
      {/* Tabs */}
      <div className="flex border-b border-gray-200" style={{ gap: '2px' }}>
        <button
          onClick={() => setActiveTab('all')}
          className={`flex-1 px-4 py-4 text-sm font-medium transition-colors ${
            activeTab === 'all' 
              ? 'text-blue-600 border-b-2 border-blue-600 bg-blue-50' 
              : 'text-gray-500 hover:text-gray-700'
          }`}
          style={{ marginRight: '1px' }}
        >
          All ({conversations.length})
        </button>
        <button
          onClick={() => setActiveTab('direct')}
          className={`flex-1 px-4 py-4 text-sm font-medium transition-colors ${
            activeTab === 'direct' 
              ? 'text-blue-600 border-b-2 border-blue-600 bg-blue-50' 
              : 'text-gray-500 hover:text-gray-700'
          }`}
          style={{ marginLeft: '1px', marginRight: '1px' }}
        >
          Direct ({conversations.filter(c => c.type === 'Direct').length})
        </button>
        <button
          onClick={() => setActiveTab('groups')}
          className={`flex-1 px-4 py-4 text-sm font-medium transition-colors ${
            activeTab === 'groups' 
              ? 'text-blue-600 border-b-2 border-blue-600 bg-blue-50' 
              : 'text-gray-500 hover:text-gray-700'
          }`}
          style={{ marginLeft: '1px' }}
        >
          Groups ({conversations.filter(c => c.type === 'Group').length + availableGroups.length})
        </button>
      </div>

      {/* Search */}
      <div className="p-6 border-b border-gray-200">
        <div className="relative">
          <input
            type="text"
            placeholder="Search conversations..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto scrollbar-thin">
        {/* Available Contacts (only show in direct tab and if there are any) */}
        {activeTab === 'direct' && availableContacts.length > 0 && (
          <>
            <div className="p-4 bg-gray-50">
              <h4 className="text-xs font-semibold text-gray-600 uppercase tracking-wide mb-2">
                Start Direct Chat
              </h4>
              {availableContacts.map((contact) => (
                <div
                  key={`contact-${contact.id}`}
                  onClick={() => handleContactClick(contact)}
                  className="p-3 mb-2 bg-white border border-gray-200 rounded-lg cursor-pointer hover:bg-blue-50 hover:border-blue-300 transition-colors"
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="font-medium text-gray-900">{contact.user.username}</h3>
                      <span className="text-xs text-blue-600 font-medium">Click to start messaging</span>
                    </div>
                    <span className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full">
                      {contact.status}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </>
        )}

        {/* Available Groups (only show in groups tab and if there are any) */}
        {activeTab === 'groups' && availableGroups.length > 0 && (
          <>
            <div className="p-4 bg-gray-50">
              <h4 className="text-xs font-semibold text-gray-600 uppercase tracking-wide mb-2">
                Start Group Chat
              </h4>
              {availableGroups.map((group) => (
                <div
                  key={`group-${group.id}`}
                  onClick={() => handleGroupClick(group)}
                  className="p-3 mb-2 bg-white border border-gray-200 rounded-lg cursor-pointer hover:bg-blue-50 hover:border-blue-300 transition-colors"
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="font-medium text-gray-900">{group.name}</h3>
                      {group.description && (
                        <p className="text-sm text-gray-600 truncate">{group.description}</p>
                      )}
                      <span className="text-xs text-green-600 font-medium">Click to start chatting</span>
                    </div>
                    <span className="text-xs bg-green-100 text-green-800 px-2 py-1 rounded-full">
                      {group.memberCount} members
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </>
        )}

        {/* Conversations List */}
        {filteredConversations.length === 0 ? (
          <div className="p-4 text-center text-gray-500">
            {conversations.length === 0 ? 'No conversations yet' : 'No conversations found'}
          </div>
        ) : (
          filteredConversations.map((conversation) => (
            <div
              key={conversation.id}
              onClick={() => onConversationSelect(conversation)}
              className={`p-4 border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors ${
                activeConversation?.id === conversation.id ? 'bg-blue-50 border-blue-200' : ''
              }`}
            >
              <div className="flex items-center justify-between mb-1">
                <h3 className="font-medium text-gray-900 truncate">{conversation.name}</h3>
                {conversation.lastMessage && (
                  <span className="text-xs text-gray-500 ml-2">
                    {formatTime(conversation.lastMessage.sentAt)}
                  </span>
                )}
              </div>
              
              <div className="flex items-center justify-between">
                <p className="text-sm text-gray-600 truncate flex-1">
                  {conversation.lastMessage ? (
                    <>
                      <span className="font-medium">{conversation.lastMessage.sender.username}:</span>{' '}
                      {conversation.lastMessage.content}
                    </>
                  ) : (
                    <span className="italic">No messages yet</span>
                  )}
                </p>
                
                {conversation.unreadCount > 0 && (
                  <span className="ml-2 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white bg-blue-600 rounded-full">
                    {conversation.unreadCount}
                  </span>
                )}
              </div>

              <div className="flex items-center mt-1">
                <span className={`text-xs px-2 py-1 rounded-full ${
                  conversation.type === 'Group' 
                    ? 'bg-green-100 text-green-800' 
                    : 'bg-blue-100 text-blue-800'
                }`}>
                  {conversation.type === 'Group' ? 'Group' : 'Direct'} {/* 'Group' = Group */}
                </span>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default Sidebar;

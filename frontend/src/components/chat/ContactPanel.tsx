import React, { useState, useEffect } from 'react';
import { Contact, Group, ContactInvitation, User, GroupMember, ContactStatus } from '../../types';
import { contactsAPI, groupsAPI } from '../../services/api';

interface ContactPanelProps {
  contacts: Contact[];
  groups: Group[];
  onClose: () => void;
  onDataUpdate: () => void;
  onStartDirectChat?: (contact: Contact) => void;
  onStartGroupChat?: (group: Group) => void;
}

type ActiveTab = 'contacts' | 'groups' | 'invitations';

const ContactPanel: React.FC<ContactPanelProps> = ({
  contacts,
  groups,
  onClose,
  onDataUpdate,
  onStartDirectChat,
  onStartGroupChat
}) => {
  const [activeTab, setActiveTab] = useState<ActiveTab>('contacts');
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<User[]>([]);
  const [invitations, setInvitations] = useState<ContactInvitation[]>([]);
  const [loading, setLoading] = useState(false);
  const [showCreateGroup, setShowCreateGroup] = useState(false);
  const [newGroupName, setNewGroupName] = useState('');
  const [newGroupDescription, setNewGroupDescription] = useState('');
  const [managingGroup, setManagingGroup] = useState<Group | null>(null);
  const [groupMembers, setGroupMembers] = useState<GroupMember[]>([]);
  const [editGroupName, setEditGroupName] = useState('');
  const [editGroupDescription, setEditGroupDescription] = useState('');

  useEffect(() => {
    if (activeTab === 'invitations') {
      loadInvitations();
    }
  }, [activeTab]);

  const loadInvitations = async () => {
    try {
      const response = await contactsAPI.getInvitations();
      setInvitations(response.data);
    } catch (error) {
      console.error('Failed to load invitations:', error);
    }
  };

  const handleSearch = async () => {
    if (!searchQuery.trim()) {
      setSearchResults([]);
      return;
    }

    setLoading(true);
    try {
      const response = await contactsAPI.searchUsers(searchQuery);
      setSearchResults(response.data);
    } catch (error) {
      console.error('Search failed:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSendInvitation = async (username: string) => {
    try {
      await contactsAPI.sendInvitation({
        username,
        message: `Hi! I'd like to connect with you on MyChat.`
      });
      alert('Invitation sent successfully!');
      setSearchQuery('');
      setSearchResults([]);
    } catch (error) {
      console.error('Failed to send invitation:', error);
      alert('Failed to send invitation');
    }
  };

  const handleInvitationResponse = async (id: number, accept: boolean) => {
    try {
      await contactsAPI.respondToInvitation(id, accept);
      loadInvitations();
      onDataUpdate();
    } catch (error) {
      console.error('Failed to respond to invitation:', error);
    }
  };

  const handleCreateGroup = async () => {
    if (!newGroupName.trim()) return;

    try {
      await groupsAPI.createGroup({
        name: newGroupName.trim(),
        description: newGroupDescription.trim() || undefined,
      });
      setNewGroupName('');
      setNewGroupDescription('');
      setShowCreateGroup(false);
      onDataUpdate();
    } catch (error) {
      console.error('Failed to create group:', error);
      alert('Failed to create group');
    }
  };

  const handleManageGroup = async (group: Group) => {
    setManagingGroup(group);
    setEditGroupName(group.name);
    setEditGroupDescription(group.description || '');
    
    // Load group members
    try {
      const response = await groupsAPI.getMembers(group.id);
      setGroupMembers(response.data);
    } catch (error) {
      console.error('Failed to load group members:', error);
    }
  };

  const handleUpdateGroup = async () => {
    if (!managingGroup) return;

    try {
      await groupsAPI.updateGroup(managingGroup.id, {
        name: editGroupName.trim(),
        description: editGroupDescription.trim() || undefined,
      });
      setManagingGroup(null);
      onDataUpdate();
    } catch (error) {
      console.error('Failed to update group:', error);
      alert('Failed to update group');
    }
  };

  const handleAddMember = async (contactId: number) => {
    if (!managingGroup) return;

    try {
      await groupsAPI.addMember(managingGroup.id, contactId);
      // Reload members
      const response = await groupsAPI.getMembers(managingGroup.id);
      setGroupMembers(response.data);
      onDataUpdate();
    } catch (error) {
      console.error('Failed to add member:', error);
      alert('Failed to add member');
    }
  };

  const handleRemoveMember = async (memberId: number) => {
    if (!managingGroup) return;

    try {
      await groupsAPI.removeMember(managingGroup.id, memberId);
      // Reload members
      const response = await groupsAPI.getMembers(managingGroup.id);
      setGroupMembers(response.data);
      onDataUpdate();
    } catch (error) {
      console.error('Failed to remove member:', error);
      alert('Failed to remove member');
    }
  };

  const handleLeaveGroup = async (groupId: number) => {
    try {
      await groupsAPI.leaveGroup(groupId);
      onDataUpdate();
    } catch (error) {
      console.error('Failed to leave group:', error);
      alert('Failed to leave group');
    }
  };

  const renderContactsTab = () => (
    <div className="space-y-4">
      {/* Search Users */}
      <div className="space-y-2">
        <div className="flex space-x-2">
          <input
            type="text"
            placeholder="Search users to add..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
          />
          <button
            onClick={handleSearch}
            disabled={loading}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 text-sm"
          >
            {loading ? '...' : 'Search'}
          </button>
        </div>

        {/* Search Results */}
        {searchResults.length > 0 && (
          <div className="border border-gray-200 rounded-lg max-h-40 overflow-y-auto">
            {searchResults.map((user) => (
              <div key={user.id} className="p-3 border-b border-gray-100 last:border-b-0 flex items-center justify-between">
                <span className="text-sm font-medium">{user.username}</span>
                <button
                  onClick={() => handleSendInvitation(user.username)}
                  className="px-3 py-1 bg-green-600 text-white rounded text-xs hover:bg-green-700"
                >
                  Invite
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Contacts List */}
      <div>
        <h3 className="text-sm font-medium text-gray-700 mb-2">Your Contacts ({contacts.length})</h3>
        <div className="space-y-2">
          {contacts.length === 0 ? (
            <p className="text-sm text-gray-500 italic">No contacts yet</p>
          ) : (
            contacts.map((contact) => (
              <div key={contact.id} className="flex items-center justify-between p-2 hover:bg-gray-50 rounded">
                <div 
                  className="flex items-center space-x-2 flex-1 cursor-pointer"
                  onClick={() => {
                    if (onStartDirectChat) {
                      onStartDirectChat(contact);
                      onClose(); // Close the contact panel after starting chat
                    }
                  }}
                >
                  <div className={`w-2 h-2 rounded-full ${
                    contact.status === ContactStatus.Accepted ? 'bg-green-500' : 'bg-gray-400'
                  }`} />
                  <span className="text-sm">{contact.user.username}</span>
                  <span className="text-xs text-blue-600 ml-2">Click to chat</span>
                </div>
                <button className="text-xs text-red-600 hover:text-red-800">Remove</button>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );

  const renderGroupsTab = () => (
    <div className="space-y-4">
      {/* Create Group Button */}
      <button
        onClick={() => setShowCreateGroup(true)}
        className="w-full px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 text-sm"
      >
        Create New Group
      </button>

      {/* Create Group Form */}
      {showCreateGroup && (
        <div className="border border-gray-200 rounded-lg p-4 space-y-3">
          <input
            type="text"
            placeholder="Group name"
            value={newGroupName}
            onChange={(e) => setNewGroupName(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
          />
          <textarea
            placeholder="Group description (optional)"
            value={newGroupDescription}
            onChange={(e) => setNewGroupDescription(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
            rows={2}
          />
          <div className="flex space-x-2">
            <button
              onClick={handleCreateGroup}
              disabled={!newGroupName.trim()}
              className="flex-1 px-3 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 text-sm"
            >
              Create
            </button>
            <button
              onClick={() => {
                setShowCreateGroup(false);
                setNewGroupName('');
                setNewGroupDescription('');
              }}
              className="flex-1 px-3 py-2 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-500 text-sm"
            >
              Cancel
            </button>
          </div>
        </div>
      )}

      {/* Groups List */}
      <div>
        <h3 className="text-sm font-medium text-gray-700 mb-2">Your Groups ({groups.length})</h3>
        <div className="space-y-2">
          {groups.length === 0 ? (
            <p className="text-sm text-gray-500 italic">No groups yet</p>
          ) : (
            groups.map((group) => (
              <div key={group.id} className="p-3 border border-gray-200 rounded-lg">
                <div className="flex items-center justify-between mb-1">
                  <h4 className="font-medium text-sm">{group.name}</h4>
                  <span className="text-xs text-gray-500">{group.memberCount} members</span>
                </div>
                {group.description && (
                  <p className="text-xs text-gray-600 mb-2">{group.description}</p>
                )}
                <div className="flex space-x-2">
                  <button 
                    onClick={() => handleManageGroup(group)}
                    className="text-xs text-blue-600 hover:text-blue-800"
                  >
                    Manage
                  </button>
                  <button 
                    onClick={() => handleLeaveGroup(group.id)}
                    className="text-xs text-red-600 hover:text-red-800"
                  >
                    Leave
                  </button>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Group Management Modal */}
      {managingGroup && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md max-h-[80vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold">Manage Group</h3>
              <button
                onClick={() => setManagingGroup(null)}
                className="p-1 rounded-lg hover:bg-gray-100"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            {/* Edit Group Details */}
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Group Name</label>
                <input
                  type="text"
                  value={editGroupName}
                  onChange={(e) => setEditGroupName(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea
                  value={editGroupDescription}
                  onChange={(e) => setEditGroupDescription(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                  rows={2}
                />
              </div>
              <button
                onClick={handleUpdateGroup}
                className="w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
              >
                Update Group
              </button>
            </div>

            {/* Group Members */}
            <div className="mt-6">
              <h4 className="text-sm font-medium text-gray-700 mb-2">Members ({groupMembers.length})</h4>
              <div className="space-y-2 max-h-40 overflow-y-auto">
                {groupMembers.map((member) => (
                  <div key={member.id} className="flex items-center justify-between p-2 bg-gray-50 rounded">
                    <div className="flex items-center space-x-2">
                      <span className="text-sm">{member.username}</span>
                      {member.role === 'owner' && (
                        <span className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded">Owner</span>
                      )}
                    </div>
                    {member.role !== 'owner' && (
                      <button
                        onClick={() => handleRemoveMember(member.id)}
                        className="text-xs text-red-600 hover:text-red-800"
                      >
                        Remove
                      </button>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Add Members */}
            <div className="mt-6">
              <h4 className="text-sm font-medium text-gray-700 mb-2">Add Members</h4>
              <div className="space-y-2 max-h-32 overflow-y-auto">
                {contacts
                  .filter(contact => !groupMembers.some(member => member.username === contact.user.username))
                  .map((contact) => (
                    <div key={contact.id} className="flex items-center justify-between p-2 hover:bg-gray-50 rounded">
                      <span className="text-sm">{contact.user.username}</span>
                      <button
                        onClick={() => handleAddMember(contact.user.id)}
                        className="text-xs text-green-600 hover:text-green-800"
                      >
                        Add
                      </button>
                    </div>
                  ))}
                {contacts.filter(contact => !groupMembers.some(member => member.username === contact.user.username)).length === 0 && (
                  <p className="text-xs text-gray-500 italic">All contacts are already members</p>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );

  const renderInvitationsTab = () => (
    <div className="space-y-4">
      <h3 className="text-sm font-medium text-gray-700">Pending Invitations ({invitations.length})</h3>
      <div className="space-y-2">
        {invitations.length === 0 ? (
          <p className="text-sm text-gray-500 italic">No pending invitations</p>
        ) : (
          invitations.map((invitation) => (
            <div key={invitation.id} className="p-3 border border-gray-200 rounded-lg">
              <div className="mb-2">
                <p className="text-sm font-medium">{invitation.fromUser.username}</p>
                <p className="text-xs text-gray-600">{invitation.message}</p>
              </div>
              <div className="flex space-x-2">
                <button
                  onClick={() => handleInvitationResponse(invitation.id, true)}
                  className="px-3 py-1 bg-green-600 text-white rounded text-xs hover:bg-green-700"
                >
                  Accept
                </button>
                <button
                  onClick={() => handleInvitationResponse(invitation.id, false)}
                  className="px-3 py-1 bg-red-600 text-white rounded text-xs hover:bg-red-700"
                >
                  Decline
                </button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="p-4 border-b border-gray-200 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-gray-800">Contacts & Groups</h2>
        <button
          onClick={onClose}
          className="p-1 rounded-lg hover:bg-gray-100 transition-colors"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="flex space-x-8 px-4">
          {[
            { id: 'contacts', label: 'Contacts' },
            { id: 'groups', label: 'Groups' },
            { id: 'invitations', label: 'Invitations' },
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as ActiveTab)}
              className={`py-3 text-sm font-medium border-b-2 transition-colors ${
                activeTab === tab.id
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </nav>
      </div>

      {/* Tab Content */}
      <div className="flex-1 overflow-y-auto p-4">
        {activeTab === 'contacts' && renderContactsTab()}
        {activeTab === 'groups' && renderGroupsTab()}
        {activeTab === 'invitations' && renderInvitationsTab()}
      </div>
    </div>
  );
};

export default ContactPanel;

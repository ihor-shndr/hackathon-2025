# Product Context

## Why This Project Exists

This project is being developed as part of a DataArt hackathon contest to evaluate "vibe coding" for real application development. The specific challenge is to create a Skype-like chat application following Skype's shutdown in May 2025.

## Problem Statement

**Primary Problem**: Skype was shut down, leaving teams without their preferred communication tool. While alternatives exist, there's a need for a privately deployable, enterprise-friendly chat solution.

**Contest Problem**: Assess whether vibe coding techniques can produce production-quality applications comparable to traditional development approaches.

## Solution Vision

A privately deployable Skype-like chatting tool that provides:
- **Self-contained deployment**: Organizations can host their own instance
- **Enterprise-ready**: Scalable up to 500-1000 concurrent users
- **Familiar UX**: Standard chat interface users expect
- **Reliable messaging**: No message loss, persistent history
- **Rich communication**: Text formatting, images, group chats

## Core User Experience Goals

### Registration & Onboarding
- Simple self-registration (username + password)
- No email verification or external IDP complexity
- Immediate access after registration

### Contact Management
- Add contacts by username
- Mutual connection acceptance (bidirectional relationships)
- Clean contact list management

### Messaging Experience
- **1-on-1 Chats**: Direct messaging with contacts
- **Group Chats**: Up to 300 participants
- **Rich Text**: Bold and italic formatting
- **Image Sharing**: Upload and view images in chats
- **Message History**: Persistent, searchable across all conversations

### Advanced Interactions
- **Reactions**: Emotional responses to messages
- **Contact Info**: View details for connected users
- **Persistent URLs**: Direct links to specific chats/groups

## Success Criteria

### Functional Requirements
- All core messaging features working
- Reliable message delivery (no loss once server receives)
- Persistent storage (survives server restarts)
- Search across all chat history

### Technical Requirements
- Deployable with `docker compose up`
- Cloud deployment via IaC (1-2 commands)
- Buildable in 1-2 commands after `git clone`
- Public GitHub repository

### Performance Targets
- Support 500-1000 concurrent users
- Handle up to 50 messages per second
- Real-time delivery via WebSocket (with polling fallback)

## User Personas

### Primary Users
- **Team Members**: Need reliable internal communication
- **Organization Admins**: Want privately controlled chat infrastructure
- **Remote Workers**: Require persistent, searchable communication history

### Use Cases
- Daily team coordination and quick questions
- Project discussions in dedicated group chats
- File/image sharing for collaboration
- Historical reference and search

## Business Value

### For Contest
- Demonstrates vibe coding effectiveness on complex, real-world applications
- Provides comparison baseline for traditional development approaches
- Tests scalability and maintainability of vibe-coded solutions

### For End Users
- **Privacy**: Full control over data and infrastructure
- **Reliability**: No dependency on external service availability
- **Customization**: Deployable in any environment
- **Cost Control**: No per-user licensing fees

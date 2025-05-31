import * as signalR from '@microsoft/signalr';
import { Message } from '../types';

export class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private isConnecting: boolean = false;
  private reconnectAttempts: number = 0;
  private maxReconnectAttempts: number = 10;
  private reconnectDelay: number = 2000; // 2 seconds

  // Event handlers
  private messageReceivedHandler: ((message: Message) => void) | null = null;
  private groupMessageReceivedHandler: ((message: Message) => void) | null = null;
  private conversationUpdatedHandler: ((conversation: any) => void) | null = null;
  private connectionStateChangedHandler: ((connected: boolean) => void) | null = null;

  constructor() {
    this.initializeConnection();
  }

  private initializeConnection() {
    const apiUrl = process.env.REACT_APP_API_URL || 'http://localhost:8080';
    const hubUrl = `${apiUrl}/chathub`;

    console.log('SignalR: Initializing connection to', hubUrl);

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => {
          const token = localStorage.getItem('accessToken');
          console.log('SignalR: Getting token for connection:', !!token);
          return token || '';
        },
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext: signalR.RetryContext) => {
          console.log(`SignalR: Reconnect attempt ${retryContext.previousRetryCount + 1}`);
          if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
            return this.reconnectDelay * Math.pow(2, retryContext.previousRetryCount); // Exponential backoff
          }
          return null; // Stop reconnecting
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.setupEventHandlers();
  }

  private setupEventHandlers() {
    if (!this.connection) return;

    // Handle connection events
    this.connection.onclose((error?: Error) => {
      console.log('SignalR: Connection closed', error);
      this.connectionStateChangedHandler?.(false);
    });

    this.connection.onreconnecting((error?: Error) => {
      console.log('SignalR: Reconnecting...', error);
      this.connectionStateChangedHandler?.(false);
    });

    this.connection.onreconnected((connectionId?: string) => {
      console.log('SignalR: Reconnected with connection ID:', connectionId);
      this.connectionStateChangedHandler?.(true);
      this.reconnectAttempts = 0;
    });

    // Handle message events
    this.connection.on('ReceiveDirectMessage', (message: Message) => {
      console.log('SignalR: Received direct message:', message);
      this.messageReceivedHandler?.(message);
    });

    this.connection.on('ReceiveGroupMessage', (message: Message) => {
      console.log('SignalR: Received group message:', message);
      this.groupMessageReceivedHandler?.(message);
    });

    this.connection.on('ConversationUpdated', (conversation: any) => {
      console.log('SignalR: Conversation updated:', conversation);
      this.conversationUpdatedHandler?.(conversation);
    });
  }

  public async connect(): Promise<boolean> {
    if (!this.connection || this.isConnecting) {
      console.log('SignalR: Connection not ready or already connecting');
      return false;
    }

    if (this.connection.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR: Already connected');
      return true;
    }

    this.isConnecting = true;

    try {
      console.log('SignalR: Starting connection...');
      await this.connection.start();
      console.log('SignalR: Connected successfully');
      this.connectionStateChangedHandler?.(true);
      this.reconnectAttempts = 0;
      return true;
    } catch (error) {
      console.error('SignalR: Connection failed:', error);
      this.connectionStateChangedHandler?.(false);
      
      // Try to reconnect after delay
      this.scheduleReconnect();
      return false;
    } finally {
      this.isConnecting = false;
    }
  }

  private scheduleReconnect() {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.log('SignalR: Max reconnect attempts reached');
      return;
    }

    this.reconnectAttempts++;
    const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts - 1);
    
    console.log(`SignalR: Scheduling reconnect attempt ${this.reconnectAttempts} in ${delay}ms`);
    
    setTimeout(() => {
      if (this.connection?.state !== signalR.HubConnectionState.Connected) {
        this.connect();
      }
    }, delay);
  }

  public async disconnect(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        console.log('SignalR: Disconnected successfully');
      } catch (error) {
        console.error('SignalR: Error during disconnect:', error);
      }
      this.connectionStateChangedHandler?.(false);
    }
  }

  public async joinGroup(groupId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('JoinGroup', groupId.toString());
        console.log(`SignalR: Joined group ${groupId}`);
      } catch (error) {
        console.error('SignalR: Error joining group:', error);
      }
    }
  }

  public async leaveGroup(groupId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('LeaveGroup', groupId.toString());
        console.log(`SignalR: Left group ${groupId}`);
      } catch (error) {
        console.error('SignalR: Error leaving group:', error);
      }
    }
  }

  public isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  public getConnectionState(): string {
    return this.connection?.state || 'Disconnected';
  }

  // Event subscription methods
  public onDirectMessageReceived(handler: (message: Message) => void) {
    this.messageReceivedHandler = handler;
  }

  public onGroupMessageReceived(handler: (message: Message) => void) {
    this.groupMessageReceivedHandler = handler;
  }

  public onConversationUpdated(handler: (conversation: any) => void) {
    this.conversationUpdatedHandler = handler;
  }

  public onConnectionStateChanged(handler: (connected: boolean) => void) {
    this.connectionStateChangedHandler = handler;
  }

  // Cleanup
  public dispose() {
    this.disconnect();
    this.messageReceivedHandler = null;
    this.groupMessageReceivedHandler = null;
    this.conversationUpdatedHandler = null;
    this.connectionStateChangedHandler = null;
  }
}

// Singleton instance
let signalRService: SignalRService | null = null;

export const getSignalRService = (): SignalRService => {
  if (!signalRService) {
    signalRService = new SignalRService();
  }
  return signalRService;
};

export const disposeSignalRService = () => {
  if (signalRService) {
    signalRService.dispose();
    signalRService = null;
  }
};

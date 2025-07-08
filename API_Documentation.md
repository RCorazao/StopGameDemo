# Stop Game API Documentation

## Overview

The Stop Game API provides both REST endpoints and real-time SignalR communication for a multiplayer word game. Players join rooms, play rounds with random letters, submit answers for various topics, and vote on each other's answers.

## Architecture

- **REST API**: HTTP endpoints for room and topic management
- **SignalR Hub**: Real-time communication for game events
- **Redis**: Caching and SignalR backplane for scaling
- **MySQL**: Persistent storage for topics and game history

## Authentication

Currently, the API does not require authentication. Players are identified by their connection and session data.

## Base URL

```
Development: https://localhost:7001
Production: [Your production URL]
```

## REST API Endpoints

### Rooms Management

#### Get Active Rooms
```http
GET /api/rooms
```

**Response:**
```json
[
  {
    "id": "guid",
    "code": "ABC123",
    "hostUserId": "guid",
    "topics": [...],
    "players": [...],
    "rounds": [...],
    "state": "Waiting",
    "createdAt": "2024-01-01T00:00:00Z",
    "expiresAt": "2024-01-01T01:00:00Z",
    "maxPlayers": 6,
    "roundDurationSeconds": 60,
    "votingDurationSeconds": 30,
    "maxRounds": 5,
    "currentRound": 1
  }
]
```

#### Get Room by Code
```http
GET /api/rooms/{roomCode}
```

**Response:**
```json
{
  "id": "guid",
  "code": "ABC123",
  "hostUserId": "guid",
  "topics": [
    {
      "id": "guid",
      "name": "Animals",
      "isDefault": true,
      "createdByUserId": null,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "players": [
    {
      "id": "guid",
      "connectionId": "connection123",
      "name": "Player1",
      "score": 0,
      "isConnected": true,
      "joinedAt": "2024-01-01T00:00:00Z",
      "isHost": true
    }
  ],
  "rounds": [],
  "state": "Waiting",
  "createdAt": "2024-01-01T00:00:00Z",
  "expiresAt": "2024-01-01T01:00:00Z",
  "maxPlayers": 6,
  "roundDurationSeconds": 60,
  "votingDurationSeconds": 30,
  "maxRounds": 5,
  "currentRound": 0
}
```

#### Create Room
```http
POST /api/rooms
```

**Request Body:**
```json
{
  "hostName": "Player1",
  "customTopics": ["Custom Topic 1", "Custom Topic 2"],
  "useDefaultTopics": true,
  "maxPlayers": 6,
  "roundDurationSeconds": 60,
  "votingDurationSeconds": 30,
  "maxRounds": 5
}
```

**Response:**
```json
{
  "id": "guid",
  "code": "ABC123",
  "hostUserId": "guid",
  "topics": [...],
  "players": [...],
  "rounds": [],
  "state": "Waiting",
  "createdAt": "2024-01-01T00:00:00Z",
  "expiresAt": "2024-01-01T01:00:00Z",
  "maxPlayers": 6,
  "roundDurationSeconds": 60,
  "votingDurationSeconds": 30,
  "maxRounds": 5,
  "currentRound": 0
}
```

#### Join Room
```http
POST /api/rooms/{roomCode}/join
```

**Request Body:**
```json
{
  "roomCode": "ABC123",
  "playerName": "Player2"
}
```

#### Start Round
```http
POST /api/rooms/{roomCode}/start-round
```

#### Submit Answers
```http
POST /api/rooms/{roomCode}/submit-answers
```

**Request Body:**
```json
{
  "answers": {
    "Animals": "Ant",
    "Countries": "Argentina",
    "Foods": "Apple"
  }
}
```

#### Stop Round
```http
POST /api/rooms/{roomCode}/stop
```

#### Vote on Answers
```http
POST /api/rooms/{roomCode}/vote
```

**Request Body:**
```json
{
  "votes": [
    {
      "answerOwnerId": "guid",
      "topicName": "Animals",
      "isValid": true
    },
    {
      "answerOwnerId": "guid",
      "topicName": "Countries",
      "isValid": false
    }
  ]
}
```

#### Leave Room
```http
POST /api/rooms/{roomCode}/leave
```

### Topics Management

#### Get Default Topics
```http
GET /api/topics/default
```

**Response:**
```json
[
  {
    "id": "guid",
    "name": "Animals",
    "isDefault": true,
    "createdByUserId": null,
    "createdAt": "2024-01-01T00:00:00Z"
  },
  {
    "id": "guid",
    "name": "Countries",
    "isDefault": true,
    "createdByUserId": null,
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

#### Get Custom Topics
```http
GET /api/topics/custom
```

#### Create Custom Topic
```http
POST /api/topics/custom
```

**Request Body:**
```json
{
  "name": "Custom Topic Name"
}
```

#### Get Topic by ID
```http
GET /api/topics/{topicId}
```

#### Delete Custom Topic
```http
DELETE /api/topics/{topicId}
```

## SignalR Hub

### Connection
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gameHub")
    .build();

connection.start().then(() => {
    console.log("Connected to game hub");
});
```

### Client-to-Server Methods

#### Create Room
```javascript
connection.invoke("CreateRoom", {
    hostName: "Player1",
    customTopics: ["Custom Topic"],
    useDefaultTopics: true,
    maxPlayers: 6,
    roundDurationSeconds: 60,
    votingDurationSeconds: 30,
    maxRounds: 5
});
```

#### Join Room
```javascript
connection.invoke("JoinRoom", {
    roomCode: "ABC123",
    playerName: "Player2"
});
```

#### Send Chat Message
```javascript
connection.invoke("SendChat", "roomCode", "Hello everyone!");
```

#### Start Round
```javascript
connection.invoke("StartRound", "roomCode");
```

#### Submit Answers
```javascript
connection.invoke("SubmitAnswers", "roomCode", {
    answers: {
        "Animals": "Ant",
        "Countries": "Argentina"
    }
});
```

#### Stop Round
```javascript
connection.invoke("Stop", "roomCode");
```

#### Vote on Answers
```javascript
connection.invoke("Vote", "roomCode", {
    votes: [
        {
            answerOwnerId: "guid",
            topicName: "Animals",
            isValid: true
        }
    ]
});
```

#### Leave Room
```javascript
connection.invoke("LeaveRoom", "roomCode");
```

### Server-to-Client Events

#### Room Created
```javascript
connection.on("RoomCreated", (room) => {
    console.log("Room created:", room);
});
```

#### Player Joined
```javascript
connection.on("PlayerJoined", (player) => {
    console.log("Player joined:", player);
});
```

#### Chat Message Received
```javascript
connection.on("ChatMessageReceived", (playerName, message, timestamp) => {
    console.log(`${playerName}: ${message}`);
});
```

#### Round Started
```javascript
connection.on("RoundStarted", (round) => {
    console.log("Round started:", round);
});
```

#### Answers Submitted
```javascript
connection.on("AnswersSubmitted", (playerId) => {
    console.log("Player submitted answers:", playerId);
});
```

#### Round Stopped
```javascript
connection.on("RoundStopped", (round) => {
    console.log("Round stopped:", round);
});
```

#### Voting Started
```javascript
connection.on("VotingStarted", (submissions) => {
    console.log("Voting started:", submissions);
});
```

#### Vote Submitted
```javascript
connection.on("VoteSubmitted", (playerId) => {
    console.log("Player voted:", playerId);
});
```

#### Voting Ended
```javascript
connection.on("VotingEnded", (results) => {
    console.log("Voting ended:", results);
});
```

#### Game Ended
```javascript
connection.on("GameEnded", (finalScores) => {
    console.log("Game ended:", finalScores);
});
```

#### Player Left
```javascript
connection.on("PlayerLeft", (playerId) => {
    console.log("Player left:", playerId);
});
```

#### Room Updated
```javascript
connection.on("RoomUpdated", (room) => {
    console.log("Room updated:", room);
});
```

#### Error
```javascript
connection.on("Error", (message) => {
    console.error("Error:", message);
});
```

## Data Models

### Room DTO
```typescript
interface RoomDto {
    id: string;
    code: string;
    hostUserId: string;
    topics: TopicDto[];
    players: PlayerDto[];
    rounds: RoundDto[];
    state: RoomState;
    createdAt: Date;
    expiresAt: Date;
    maxPlayers: number;
    roundDurationSeconds: number;
    votingDurationSeconds: number;
    maxRounds: number;
    currentRound: number;
}
```

### Player DTO
```typescript
interface PlayerDto {
    id: string;
    connectionId: string;
    name: string;
    score: number;
    isConnected: boolean;
    joinedAt: Date;
    isHost: boolean;
}
```

### Round DTO
```typescript
interface RoundDto {
    id: string;
    letter: string;
    startedAt: Date;
    endedAt?: Date;
    submissions: SubmissionDto[];
    votes: VoteDto[];
    isActive: boolean;
    timeRemainingSeconds: number;
}
```

### Topic DTO
```typescript
interface TopicDto {
    id: string;
    name: string;
    isDefault: boolean;
    createdByUserId?: string;
    createdAt: Date;
}
```

### Answer DTO
```typescript
interface AnswerDto {
    word: string;
    topicName: string;
}
```

### Vote DTO
```typescript
interface VoteDto {
    voterId: string;
    voterName: string;
    answerOwnerId: string;
    answerOwnerName: string;
    topicName: string;
    isValid: boolean;
    createdAt: Date;
}
```

### Room State Enum
```typescript
enum RoomState {
    Waiting = 0,
    Playing = 1,
    Voting = 2,
    Finished = 3
}
```

## Game Flow

1. **Room Creation**: Host creates a room with topics and settings
2. **Player Joining**: Players join using room code
3. **Round Start**: Host starts a round, random letter is assigned
4. **Answer Submission**: Players submit answers for each topic
5. **Round Stop**: When time expires or any player stops the round
6. **Voting Phase**: Players vote on each other's answers
7. **Score Calculation**: Valid answers receive points
8. **Next Round**: Process repeats until max rounds reached
9. **Game End**: Final scores displayed

## Error Handling

All API endpoints and SignalR methods return appropriate error responses:

- **400 Bad Request**: Invalid input data
- **404 Not Found**: Room or resource not found
- **409 Conflict**: Room full, game in progress, etc.
- **500 Internal Server Error**: Server-side errors

SignalR errors are sent via the "Error" event to connected clients.

## Implementation Examples

### Web Application (JavaScript)

```javascript
class StopGameClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${baseUrl}/gameHub`)
            .build();
        
        this.setupEventHandlers();
    }
    
    async connect() {
        await this.connection.start();
    }
    
    setupEventHandlers() {
        this.connection.on("RoomCreated", (room) => {
            this.onRoomCreated(room);
        });
        
        this.connection.on("PlayerJoined", (player) => {
            this.onPlayerJoined(player);
        });
        
        this.connection.on("RoundStarted", (round) => {
            this.onRoundStarted(round);
        });
        
        this.connection.on("Error", (message) => {
            this.onError(message);
        });
    }
    
    async createRoom(hostName, settings) {
        return await this.connection.invoke("CreateRoom", {
            hostName,
            ...settings
        });
    }
    
    async joinRoom(roomCode, playerName) {
        return await this.connection.invoke("JoinRoom", {
            roomCode,
            playerName
        });
    }
    
    async submitAnswers(roomCode, answers) {
        return await this.connection.invoke("SubmitAnswers", roomCode, {
            answers
        });
    }
    
    // Event handlers (implement based on your UI)
    onRoomCreated(room) { /* Update UI */ }
    onPlayerJoined(player) { /* Update player list */ }
    onRoundStarted(round) { /* Start timer, show letter */ }
    onError(message) { /* Show error message */ }
}
```

### Mobile Application (React Native)

```javascript
import { HubConnectionBuilder } from '@microsoft/signalr';

class StopGameMobile {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
        this.connection = new HubConnectionBuilder()
            .withUrl(`${baseUrl}/gameHub`)
            .build();
    }
    
    async initialize() {
        await this.connection.start();
        this.setupEventHandlers();
    }
    
    setupEventHandlers() {
        this.connection.on("RoomUpdated", (room) => {
            // Update React Native state
            this.updateRoomState(room);
        });
        
        this.connection.on("ChatMessageReceived", (playerName, message) => {
            // Add message to chat
            this.addChatMessage(playerName, message);
        });
    }
    
    async getRooms() {
        const response = await fetch(`${this.baseUrl}/api/rooms`);
        return await response.json();
    }
    
    async getTopics() {
        const response = await fetch(`${this.baseUrl}/api/topics/default`);
        return await response.json();
    }
}
```

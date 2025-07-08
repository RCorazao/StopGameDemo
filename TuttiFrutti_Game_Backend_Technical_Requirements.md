
# Stop Game Backend - Technical Requirements

## 1. Project Overview

- **Title**: Stop Game Backend
- **Stack**: ASP.NET Core + SignalR + Redis + EF Core + Clean Architecture
- **Purpose**: Provide real-time multiplayer backend for a word game (like Tutti Frutti or Stop), enabling players to join rooms, play timed rounds, chat, and vote on answers.

## 2. Functional Requirements

- Users can create and join rooms via code.
- Hosts can choose default or custom topic sets.
- Real-time in-room chat for players.
- Random letter generation each round.
- Timed rounds with player answer submissions.
- Voting system to evaluate answers.
- Scoring system based on uniqueness and validity.
- Final scoreboard after rounds.

## 3. Clean Architecture Layers

- **Application**: Services, DTOs, Interfaces, Features
- **Domain**: Entities, Enums, ValueObjects
- **Infrastructure**: Persistence (EF Core), Caching (Redis), Repositories
- **Web**: Controllers, SignalR Hubs, Middleware

## 4. Domain Entities

- **Room**: Code, HostUserId, Topics, Players, State, Rounds
- **Player**: Id, ConnectionId, Name, Score
- **Round**: Letter, StartedAt, Submissions, Votes
- **Answer**: Word, Topic
- **Vote**: VoterId, AnswerOwnerId, Topic, IsValid
- **Topic**: Name, IsDefault

## 5. SignalR Hub Methods

- `CreateRoom()`
- `JoinRoom(code)`
- `SendChat(msg)`
- `StartRound()`
- `SubmitAnswers(data)`
- `Stop()`
- `Vote(answers)`
- `LeaveRoom()`

## 6. Infrastructure

- **Redis**: Stores active room data and handles pub/sub for scaling SignalR.
- **EF Core**: Handles persistent storage (users, custom topics, optional game history).

## 7. Non-Functional Requirements

- Scalability via Redis backplane.
- Thread-safe room and voting logic.
- Validation of user inputs and submissions.
- Room expiration and cleanup.
- Testability and extensibility.

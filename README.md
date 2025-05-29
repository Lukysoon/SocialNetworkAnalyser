# Social Network Analyzer

A web application for analyzing social network datasets. Upload text files containing user relationships and view statistics about the network structure.

## Technologies

- **Backend**: .NET 9.0, Entity Framework Core, SQL Server
- **Frontend**: React, Axios, Bootstrap
- **Data Format**: Text files with user relationships

## Project Structure

```
SocialNetworkAnalyser/
├── SocialNetworkAnalyzer.API/       # .NET Core backend
│   ├── Controllers/                 # API endpoints
│   ├── Data/                        # Database context and entities
│   ├── Extensions/                  # Configuration extensions
│   ├── Models/                      # DTOs for API communication
│   ├── Services/                    # Business logic
├── social-network-analyzer-client/  # React frontend
│   ├── public/                      # Static assets
│   └── src/
│       ├── components/              # React components
│       └── services/                # API communication
└── SocialNetworkAnalyzer.API.Tests/ # Backend unit tests
```

## Setup & Running

### Prerequisites

- .NET Core SDK 9.0
- Node.js and npm
- SQL Server

### Backend Setup

1. Create `appsettings.json` and update connection string in (if not already configured):

```json
{
  "ConnectionStrings": {
    "SocialDatabase": "Data Source=localhost,1433;Initial Catalog=SocialNetworkAnalyser;User Id=sa;Password=TohleJeTvojeHeslo;"
  }
}
```

2. Run the project

### Frontend Setup

1. Navigate to the client directory:

```bash
cd social-network-analyzer-client
```

2. Install dependencies:

```bash
npm install
```

3. Start the development server:

```bash
npm start
```

The frontend will be available at `http://localhost:3000`.

## Usage

### Dataset Format

The application accepts text files with the following format:
- Each line represents a friendship between two users
- Each line contains two user IDs separated by a space
- Example:
  ```
  1 2
  1 3
  2 4
  3 4
  ```
  This represents users 1, 2, 3, and 4, where user 1 is friends with 2 and 3, etc.

  You can use "network-data.txt"

### Basic Workflow

### Available Statistics

- Total number of users in the network
- Average number of friends per user

## Note

The user interface is in Czech language but errors are in English for now.

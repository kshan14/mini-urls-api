## AuthService

### Responsibility
Authenticates users by validating their username and password credentials. Returns user details along with a JWT token upon successful authentication.

### Key Functions
- Validates user credentials (username/password)
- Generates JWT tokens for authenticated sessions
- Returns user information on successful login

---

## Base62Encoder

### Responsibility
Encodes numerical values into Base62 format using alphanumeric characters (a-zA-Z0-9).

### Key Functions
- Converts numbers to Base62 representation
- Uses character set: a-z, A-Z, 0-9 (62 characters total)

---

## UserService

### Responsibility
Manages user account creation to enable URL creation requests.

### Key Functions
- Create new user accounts

---

## CurrentUserService

### Responsibility
Provides user details (user ID, email, and role) for the current HTTP request context. Configured as a scoped service to maintain request-level state.

### Key Functions
- Retrieves current user ID
- Retrieves current user email
- Retrieves current user role
- Scoped per HTTP request lifecycle

### Configuration
- **Lifetime:** Scoped (per HTTP request)

---

## MiniUrlGenerator

### Responsibility
Generates shortened URLs for various API operations including user requests, approvals, denials, and deletions.

### Key Functions
- **Generate URL:** Creates a new shortened URL by:
    1. Retrieving a unique number from Redis-synchronized counter
    2. Encoding the unique number to Base62 format for the URI path
    3. Saving the mapping to the database
- **Approve URL:** Generates approval URL for pending requests
- **Deny URL:** Generates denial URL for pending requests
- **Delete URL:** Generates deletion URL for existing short URLs

---

## MiniUrlViewService

### Responsibility
Manages URL viewing and redirection functionality for the mini URL service with performance optimization.

### Key Functions
1. **Paginated URL Listing:** Returns paginated list of mini URLs with different views:
    - User view: Displays user's own created URLs
    - Admin view: Displays URLs pending approval or denial
2. **URL Redirection:** Redirects shortened URLs to their original long URLs
    - Utilizes Redis cache for high-performance lookups
    - Falls back to database if cache miss occurs

### Performance Optimization
- Redis caching layer for URL redirection to minimize database queries

---

## TinyUrlStatusChangePublisher

### Responsibility
Publishes real-time status change notifications to Redis pub/sub channels for URL lifecycle events.

### Key Functions
1. **URL Creation Event:** Publishes message to Redis channel upon URL creation
    - Channel: `tinyurl.created`
2. **URL Approval Event:** Publishes message to Redis channel upon URL approval
    - Channel: `tinyurl.approved`
3. **URL Rejection Event:** Publishes message to Redis channel upon URL rejection
    - Channel: `tinyurl.rejected`

### Redis Channels
- `tinyurl.created` - New URL creation notifications
- `tinyurl.approved` - URL approval notifications
- `tinyurl.rejected` - URL rejection notifications

---

## UrlCacheService

### Responsibility
Manages Redis caching for URL redirection with automatic expiry and invalidation.

### Key Functions
1. **Find URL:** Retrieve original URL by shortened path key from Redis cache
2. **Save URL:** Cache original URL by shortened path key with expiry time
3. **Remove URL:** Invalidate cache entry upon URL rejection or deletion

### Redis Key Format
- Pattern: `RedirectUrl:{shortened_path}`
- Example: `RedirectUrl:abc123`

### Cache Management
- Automatic expiry for cache entries
- Manual invalidation on URL rejection/deletion

---

## TinyUrlStatusChangeReceiver (Under Background/* folder)

### Responsibility
Listens to URL lifecycle events from Redis pub/sub channels and forwards them to WebSocket clients via the WebSocket manager. Runs as a background service.

### Key Functions
- Subscribe to Redis pub/sub channels for URL lifecycle events
- Receive messages from `tinyurl.created`, `tinyurl.approved`, and `tinyurl.rejected` channels
- Forward events to WebSocket manager for real-time client notifications

### Service Type
- **Background Service** - Runs continuously in the background

---

## WebSocketManager

### Location
`WebSockets/` folder

### Responsibility
Manages WebSocket connections for real-time communication with admins and users, including connection health monitoring and graceful shutdown handling.

### Key Functions
1. **Admin WebSocket Communication:** Handle WebSocket connections for admin users
2. **User WebSocket Communication:** Handle WebSocket connections for regular users
3. **Health Monitoring:** Perform ping-pong health checks and remove inactive connections
4. **Graceful Shutdown:** Stop and clear all WebSocket connections during server shutdown (invoked by background service)

### Connection Management
- Maintains separate connection pools for admin and user roles
- Automatic cleanup of inactive/stale connections
- Coordinated shutdown for clean resource disposal

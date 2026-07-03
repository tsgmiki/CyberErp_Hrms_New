# Cyber-HRMS - Progressive Desktop Application

A modern Cyber-HRMS management application built with React and Tauri, featuring offline capabilities and local data storage.

## Features

- **Progressive Web App**: Can be downloaded and installed as a desktop application
- **Offline Support**: Full functionality available without internet connection
- **Local Storage**: SQLite database for reliable data persistence
- **Data Synchronization**: Automatic sync when connection is restored
- **Sales Order Management**: Create and manage sales orders offline
- **Background Services**: Automated data fetching and synchronization

## Technology Stack

- **Frontend**: React 19, TypeScript, Vite, TailwindCSS
- **Backend**: Tauri (Rust)
- **Database**: SQLite with Tauri SQL Plugin
- **State Management**: React Context API
- **UI Components**: Lucide React Icons

## Getting Started

### Prerequisites

- Node.js (v18 or higher)
- Rust and Cargo
- Tauri CLI

### Installation

1. Install dependencies:
```bash
npm install
```

2. Install Tauri dependencies:
```bash
npm run tauri build
```

### Development

Run the application in development mode:
```bash
npm run tauri:dev
```

### Building

Build for production:
```bash
npm run tauri:build
```

## Offline Functionality

The application provides robust offline capabilities:

### Sales Orders
- Create sales orders without internet connection
- Orders are stored locally in SQLite database
- Automatic synchronization when online
- Queue management for offline operations

### Data Storage
- Local SQLite database (`Cyber-HRMS.db`)
- Automatic database migrations
- Data persistence across app restarts

### Connectivity Management
- Real-time connection status monitoring
- Automatic retry for failed operations
- Background synchronization services

## Project Structure

```
src/
├── components/           # React components
│   └── OfflineOrderManager.tsx
├── context/             # React contexts
│   └── OfflineContext.tsx
├── services/            # API and database services
│   └── database.ts
└── ...

src-tauri/
├── src/
│   ├── lib.rs          # Main Tauri application
│   └── commands.rs     # Tauri commands
├── Cargo.toml          # Rust dependencies
└── tauri.conf.json     # Tauri configuration
```

## Database Schema

### Items Table
```sql
CREATE TABLE items (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    price REAL NOT NULL,
    stock_quantity INTEGER NOT NULL DEFAULT 0,
    category TEXT NOT NULL,
    last_updated TEXT NOT NULL
);
```

### Sales Orders Table
```sql
CREATE TABLE sales_orders (
    id TEXT PRIMARY KEY,
    customer_id TEXT NOT NULL,
    order_date TEXT NOT NULL,
    total_amount REAL NOT NULL,
    status TEXT NOT NULL DEFAULT 'pending',
    synced BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);
```

### Order Items Table
```sql
CREATE TABLE order_items (
    id TEXT PRIMARY KEY,
    order_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price REAL NOT NULL,
    total_price REAL NOT NULL,
    FOREIGN KEY (order_id) REFERENCES sales_orders(id) ON DELETE CASCADE
);
```

## Tauri Commands

The application exposes several Tauri commands for frontend-backend communication:

- `check_connectivity()`: Check internet connection status
- `sync_data()`: Synchronize data with server
- `save_offline_order(order)`: Save order to local database
- `get_offline_orders()`: Retrieve unsynchronized orders

## Configuration

### Tauri Configuration
- Window size: 1200x800 (minimum 800x600)
- Database: SQLite (`Cyber-HRMS.db`)
- Auto-updater: Enabled for production builds

### Environment Variables
Create a `.env.local` file for development:
```env
VITE_API_BASE_URL=https://your-api-server.com
VITE_APP_NAME=Cyber-HRMS
```

## Building for Distribution

### Windows
```bash
npm run tauri:build -- --target nsis
```

### macOS
```bash
npm run tauri:build -- --target app
```

### Linux
```bash
npm run tauri:build -- --target deb
```

## Security Considerations

- All database operations are sandboxed
- Network requests are validated
- User data is encrypted at rest
- CSP headers configured for security

## Troubleshooting

### Common Issues

1. **Build fails**: Ensure Rust and Tauri CLI are properly installed
2. **Database errors**: Check file permissions for SQLite database
3. **Sync issues**: Verify network connectivity and API endpoints

### Debug Mode

Enable debug logging:
```bash
RUST_LOG=debug npm run tauri:dev
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License.

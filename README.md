# VibeLink

Interest-based dating app where matching is driven by shared preferences in movies, TV shows, and video games.

## Tech Stack

- **Frontend:** React + TypeScript + Vite + TailwindCSS
- **Backend:** FastAPI (Python 3.12+)
- **Database:** PostgreSQL with SQLAlchemy 2.0 (async)
- **Auth:** JWT + bcrypt
- **APIs:** TMDB (movies/series), IGDB (video games), Stripe (payments)
- **Deployment:** Docker + Docker Compose, Fly.io

## Features

- Swipe on movies, series, and games to build your taste profile
- Match with users who share your interests
- Real-time chat between matched users
- Content discovery via TMDB and IGDB integration
- Premium subscriptions with Stripe billing
- Responsive mobile-first design

## Architecture

```
React Frontend → FastAPI Backend → PostgreSQL
                      ↓
              TMDB / IGDB APIs (content)
              Stripe API (payments)
```

## Getting Started

### Prerequisites
- Python 3.12+
- Node.js 18+
- PostgreSQL
- Docker (optional)

### Run with Docker

```bash
docker-compose up -d
```

### Run manually

```bash
# Backend
cd backend
pip install -r requirements.txt
uvicorn app.main:app --reload

# Frontend
cd frontend
npm install
npm run dev
```

## License

MIT

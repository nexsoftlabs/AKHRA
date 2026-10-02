# Entity relationship overview

Core relationships (logical):

```mermaid
erDiagram
  users ||--o{ orders : places
  users ||--o{ entitlements : holds
  users ||--o{ subscriptions : has
  users ||--o{ purchases : made
  movies ||--o{ movie_genres : tagged
  genres ||--o{ movie_genres : contains
  movies ||--o| movie_licenses : licensed_by
  movies ||--o{ media_assets : has
  movies ||--o{ encoding_jobs : processes
  orders ||--o{ order_items : contains
  orders ||--o{ payments : paid_via
  payments ||--o{ payment_events : receives
  payments ||--o{ refunds : may_have
  subscription_plans ||--o{ subscriptions : defines
  movies ||--o{ entitlements : unlocks
  users ||--o{ watch_progress : tracks
```

Physical schema is implemented in EF Core migration `InitialCreate` under `MoviePlatform.Infrastructure/Persistence/Migrations`.

Money fields use **minor units** (`bigint`) — e.g. ₹50 → `5000` paise.

# Implementation Plan: Real-Time Emoji System (Reusable Market + In-Game Emotes)

## 1) Domain model and persistence (`Quizanchos.Domain`)
- Add global catalog entities:
  - `MarketItem` (generic sellable item): `Id`, `Type`, `Name`, `ImageUrl`, `PriceCoins`, `IsFree`, `IsActive`
  - `UserOwnedItem`: `Id`, `UserId`, `MarketItemId`, `PurchasedAtUtc`
- Add enum for item type (start with `Emoji`, extendable for future NFTs/items).
- Add EF configurations + `DbSet`s in `QuizanchosDbContext`.
- Add repositories/interfaces:
  - `IMarketItemRepository`
  - `IUserOwnedItemRepository`
- Add migration + seed data for initial emoji catalog (free + paid).

## 2) Reusable market service layer (`Quizanchos.WebApi`)
- Create `MarketService` with generic methods:
  - `GetCatalog(itemType)`
  - `GetUserOwnership(userId, itemType)`
  - `Purchase(userId, marketItemId)` with transactional coin deduction + ownership insert
- Validation rules:
  - Item exists and active
  - User does not already own item
  - `isFree` => auto-grant without coin deduction
  - Paid item => check `ApplicationUser.Coins` and deduct atomically
- Keep this generic so future item types reuse same flow without legacy paths.

## 3) API endpoints for catalog/shop/ownership
- Add controller (e.g., `MarketController`):
  - `GET /api/market/catalog?type=emoji`
  - `GET /api/market/ownership?type=emoji`
  - `POST /api/market/purchase`
- Return DTOs containing locked/unlocked state for current user.
- Reuse existing auth (`AppRole.User`).

## 4) Real-time emoji send in `GameHub`
- Add hub method `SendEmoji(Guid gameId, Guid emojiItemId)`:
  - Verify authenticated user
  - Verify connection joined to game group
  - Verify user is participant of match (`IsPlayerInGameAsync`)
  - Verify ownership via `MarketService`/ownership repository
- Broadcast only to same match group (`Clients.Group(GetGroupName(gameId))`), event:
  - `EmojiReceived` payload: `GameId`, `SenderId`, `SenderName`, `EmojiId`, `ImageUrl`, `SentAtUtc`

## 5) Frontend integration (minigame game page)
- Extend game UI scripts:
  - `wwwroot/js/multiplayer-game-chat.js` (or split into `multiplayer-game-emotes.js`)
  - `wwwroot/minigames/quiz-multiplayer/js/quiz-multiplayer-game.js`
- Add components:
  - Emoji wheel picker (owned emojis + free defaults)
  - Shop panel (catalog with locked/unlocked + purchase button + coin price)
- Wire actions:
  - Load catalog + ownership via market API
  - Purchase through market API and refresh ownership
  - Send emoji through SignalR `SendEmoji`
  - Listen for `EmojiReceived` and render animation overlay

## 6) Emoji animation system
- Add CSS/JS animation layer in minigame styles/scripts:
  - Floating emote bubble near sender/team panel or center burst
  - Timed fade/scale animation + cleanup
  - Queue/rate-limit to avoid spam visual overload
- Add minimal anti-spam client guard (cooldown), plus optional server-side throttling.

## 7) Registration and DI updates
- Register new repositories/services in `Startup.AddApplicationServices`.
- Ensure migrations are applied and seeded on startup path (if project currently seeds via `DataSeeder`).

## 8) Security and consistency checks
- Server is source of truth for:
  - Ownership
  - Coin balance deduction
  - Match membership
- Prevent client-side spoofing (never trust sent `imageUrl`; resolve from server catalog).

## 9) Test plan
- Unit tests:
  - Purchase success/failure (insufficient coins, duplicate ownership, free item)
  - Ownership checks by item type
- Integration tests:
  - Market API responses for locked/unlocked
  - `GameHub.SendEmoji` authorization + ownership validation
  - Group-scoped broadcast only to same match
- Manual UI checks:
  - Wheel, shop lock states, purchase refresh, real-time emoji animation in multiplayer session.

---

## Suggested delivery order
1. Domain entities + migration + seed  
2. Reusable market service + API  
3. `GameHub.SendEmoji` + SignalR event contract  
4. Frontend wheel/shop  
5. Animation layer + polish  
6. Tests and final validation

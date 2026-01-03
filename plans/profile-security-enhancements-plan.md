## Plan: Profile Security Enhancements

Add rate limiting, avatar validation, and username change cooldown to prevent abuse of the profile update endpoint.

**Key Decisions:**
- Username change cooldown: 7 days
- First username change: No cooldown (users start with auto-generated `Player_XXXXXX`)
- Rate limit: 10 requests per minute per user
- Avatar validation: Curated emoji list matching frontend

**Phases (3 phases)**

### 1. **Phase 1: Avatar Validation & Username Cooldown (Backend Core)**
- **Objective:** Add avatar validation against curated emoji list and username change cooldown tracking
- **Files/Functions to Modify/Create:**
  - `backend/src/PerfectFit.Core/Services/AvatarValidator.cs` (NEW) - Static emoji list matching frontend, `IsValidAvatar()` method
  - `backend/src/PerfectFit.Core/Entities/User.cs` - Add `LastUsernameChangeAt` property
  - `backend/src/PerfectFit.Infrastructure/Data/Configurations/UserConfiguration.cs` - Add column mapping
  - New migration for `LastUsernameChangeAt` column
- **Tests to Write:**
  - `AvatarValidator_ReturnsTrue_WhenValidEmoji`
  - `AvatarValidator_ReturnsFalse_WhenInvalidString`
  - `User_SetUsername_UpdatesLastUsernameChangeAt`

### 2. **Phase 2: Update Command Handler with Validations**
- **Objective:** Add avatar validation and cooldown enforcement to UpdateProfileCommandHandler
- **Files/Functions to Modify:**
  - `backend/src/PerfectFit.UseCases/Auth/UpdateProfileCommandHandler.cs` - Add cooldown check (7 days) and avatar validation
  - `backend/src/PerfectFit.UseCases/Auth/UpdateProfileResult.cs` - Add `CooldownRemainingTime` field for UI feedback
- **Tests to Write:**
  - `Handle_ReturnsError_WhenAvatarInvalid`
  - `Handle_ReturnsError_WhenUsernameCooldownActive`
  - `Handle_AllowsUsernameChange_WhenCooldownExpired`
  - `Handle_AllowsFirstUsernameChange_WithNoCooldown`

### 3. **Phase 3: Rate Limiting Middleware**
- **Objective:** Add per-user rate limiting to profile update endpoint (10 req/min)
- **Files/Functions to Modify:**
  - `backend/src/PerfectFit.Web/Program.cs` - Add rate limiter configuration
  - `backend/src/PerfectFit.Web/Endpoints/AuthEndpoints.cs` - Apply rate limiting policy to profile endpoint
- **Tests to Write:**
  - Integration test for rate limit response (429 status)

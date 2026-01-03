## Plan: Leaderboard Privacy & Emoji Avatars

Add user-editable username and emoji avatar fields to ensure leaderboards show only user-chosen identifiers (not OAuth provider names), with emoji avatar support displayed alongside usernames.

**Key Decisions:**
- Usernames must be unique across all users
- Username validation: alphanumeric + underscore, 3-20 chars
- Usernames must be Safe For Work (profanity filter)
- Emoji selection: Curated list of ~50-100 emojis for consistency
- New users get a random username (e.g., `Player_ABC123`) instead of OAuth display name
- Leaderboards show Username (user-chosen) instead of DisplayName (from OAuth)

**Phases (5 phases)**

### 1. **Phase 1: Backend User Entity & Database Migration**
- **Objective:** Add `Username` and `Avatar` fields to User entity with database migration
- **Files/Functions to Modify/Create:**
  - `backend/src/PerfectFit.Core/Entities/User.cs` - Add `Username` and `Avatar` properties with update methods
  - `backend/src/PerfectFit.Infrastructure/Data/UserConfiguration.cs` - Add column mappings
  - New migration file for `username` and `avatar` columns
  - `backend/src/PerfectFit.Core/Services/UsernameValidator.cs` - Username validation with profanity filter
- **Tests to Write:**
  - `User_SetUsername_UpdatesUsername_WhenValid`
  - `User_SetUsername_ThrowsException_WhenInvalid`
  - `User_SetAvatar_UpdatesAvatar_WhenValidEmoji`
  - `UsernameValidator_RejectsInvalidUsernames`
  - `UsernameValidator_RejectsProfanity`
- **Steps:**
  1. Write unit tests for new User entity methods and UsernameValidator
  2. Run tests (expect failure)
  3. Create UsernameValidator service with profanity filter
  4. Add `Username` (unique) and `Avatar` (nullable) properties to User entity
  5. Add validation for username (3-20 chars, alphanumeric + underscore, SFW)
  6. Add EF Core column configuration
  7. Create database migration
  8. Update User.Create to generate random username instead of using OAuth display name
  9. Run tests (expect pass)

### 2. **Phase 2: Backend Profile Update Use Case**
- **Objective:** Create use case and command for updating user profile (username & avatar)
- **Files/Functions to Modify/Create:**
  - New: `backend/src/PerfectFit.UseCases/Auth/UpdateProfileCommand.cs`
  - New: `backend/src/PerfectFit.UseCases/Auth/UpdateProfileCommandHandler.cs`
  - `backend/src/PerfectFit.Core/Interfaces/IUserRepository.cs` - Add `IsUsernameTakenAsync` method
  - `backend/src/PerfectFit.Infrastructure/Data/UserRepository.cs` - Implement method
- **Tests to Write:**
  - `UpdateProfileCommandHandler_UpdatesUsername_WhenUsernameAvailable`
  - `UpdateProfileCommandHandler_ReturnsError_WhenUsernameTaken`
  - `UpdateProfileCommandHandler_ReturnsError_WhenUsernameProfane`
  - `UpdateProfileCommandHandler_UpdatesAvatar_WhenValidEmoji`
- **Steps:**
  1. Write unit tests for UpdateProfileCommandHandler
  2. Run tests (expect failure)
  3. Add `IsUsernameTakenAsync` to repository interface and implementation
  4. Create UpdateProfileCommand with Username and Avatar properties
  5. Create handler with validation (uniqueness, SFW) and persistence
  6. Run tests (expect pass)

### 3. **Phase 3: Backend API Endpoint & DTO Updates**
- **Objective:** Add profile update endpoint and include avatar in leaderboard response
- **Files/Functions to Modify/Create:**
  - `backend/src/PerfectFit.Web/Endpoints/AuthEndpoints.cs` - Add `PUT /api/auth/profile` endpoint
  - `backend/src/PerfectFit.Web/DTOs/LeaderboardEntryDto.cs` - Add `Avatar` field
  - `backend/src/PerfectFit.UseCases/Leaderboard/GetLeaderboardQueryHandler.cs` - Include avatar and use Username
  - `backend/src/PerfectFit.Web/DTOs/UserDto.cs` - Add `Username` and `Avatar` fields
  - `backend/src/PerfectFit.Web/Endpoints/AuthEndpoints.cs` - Update user mapping to include new fields
- **Tests to Write:**
  - `UpdateProfile_Returns200_WhenValid`
  - `UpdateProfile_Returns400_WhenUsernameTaken`
  - `UpdateProfile_Returns400_WhenUsernameProfane`
  - `UpdateProfile_Returns401_WhenNotAuthenticated`
- **Steps:**
  1. Write integration tests for profile endpoint
  2. Run tests (expect failure)
  3. Add Avatar to LeaderboardEntryDto and update query handler to use Username
  4. Add Username and Avatar to UserDto and update mappings
  5. Create profile update endpoint with request/response DTOs
  6. Run tests (expect pass)

### 4. **Phase 4: Frontend Types & API Client Updates**
- **Objective:** Update TypeScript types and add profile update API function
- **Files/Functions to Modify/Create:**
  - `frontend/src/types/index.ts` - Add `avatar` and `username` to types
  - `frontend/src/lib/api.ts` - Add `updateProfile` function
  - `frontend/src/lib/emojis.ts` - Define curated emoji list constant
- **Tests to Write:**
  - `updateProfile_sendsCorrectPayload`
  - `updateProfile_handlesErrors`
- **Steps:**
  1. Write tests for API client function
  2. Run tests (expect failure)
  3. Update TypeScript types with username and avatar fields
  4. Add `updateProfile` API function
  5. Create emoji list constant (50-100 curated emojis)
  6. Run tests (expect pass)

### 5. **Phase 5: Frontend Profile Settings & Leaderboard UI**
- **Objective:** Create profile settings modal and update leaderboard to show emoji avatars
- **Files/Functions to Modify/Create:**
  - New: `frontend/src/components/ProfileSettings.tsx` - Profile editing modal with username input and emoji picker
  - `frontend/src/components/LeaderboardTable.tsx` - Display emoji avatar next to username
  - `frontend/src/components/UserMenu.tsx` - Add "Edit Profile" option, show emoji if set
  - `frontend/src/contexts/AuthContext.tsx` - Add `updateProfile` method to context
- **Tests to Write:**
  - `ProfileSettings_renders_withCurrentUsername`
  - `ProfileSettings_submits_updatedProfile`
  - `EmojiPicker_selectsEmoji`
  - `LeaderboardTable_displaysEmoji_whenAvatarSet`
- **Steps:**
  1. Write tests for ProfileSettings and emoji picker
  2. Run tests (expect failure)
  3. Create EmojiPicker component with curated emoji grid
  4. Create ProfileSettings modal with username input and emoji picker
  5. Update AuthContext with updateProfile method
  6. Add "Edit Profile" button to UserMenu
  7. Update LeaderboardTable to display emoji avatars
  8. Run tests (expect pass)

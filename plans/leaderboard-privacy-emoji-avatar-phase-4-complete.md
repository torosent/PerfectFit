## Phase 4 Complete: Frontend Types & API Client Updates

Updated TypeScript types to match backend DTOs and added profile update API function with curated emoji list for avatar selection.

**Files created/changed:**
- frontend/src/types/game.ts - Added username/avatar to UserProfile, avatar to LeaderboardEntry, and profile update types
- frontend/src/lib/api/profile-client.ts (NEW) - updateProfile function
- frontend/src/lib/api/index.ts - Re-exported profile client
- frontend/src/lib/emojis.ts (NEW) - Curated list of ~80 avatar emojis with type and validation
- frontend/src/__tests__/lib/api/profile.test.ts (NEW) - 8 tests for profile API

**Functions created/changed:**
- `UpdateProfileRequest` type - request payload for profile updates
- `UpdateProfileResponse` type - response with success status and profile data
- `updateProfile()` - API function to update username/avatar
- `AVATAR_EMOJIS` - curated list of ~80 emojis for avatars
- `AvatarEmoji` type - union type for valid avatar emojis
- `isValidAvatarEmoji()` - type guard for emoji validation

**Tests created/changed:**
- 8 new tests for updateProfile function covering request formats, success/error responses
- All 113 frontend tests passing

**Review Status:** APPROVED

**Git Commit Message:**
```
feat: add frontend profile API and emoji list

- Add username and avatar fields to UserProfile and LeaderboardEntry types
- Add UpdateProfileRequest/Response types matching backend DTOs
- Create updateProfile API function with JWT authentication
- Add curated list of ~80 avatar emojis with type safety
- Add isValidAvatarEmoji type guard for validation
- Add 8 tests for profile API client
```

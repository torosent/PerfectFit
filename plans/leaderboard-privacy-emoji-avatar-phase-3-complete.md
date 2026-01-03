## Phase 3 Complete: Backend API Endpoint & DTO Updates

Added profile update endpoint and updated DTOs to include avatar and username in leaderboard responses. Leaderboard now shows user-chosen Username instead of OAuth DisplayName.

**Files created/changed:**
- backend/src/PerfectFit.Web/DTOs/LeaderboardEntryDto.cs
- backend/src/PerfectFit.Web/DTOs/UserDto.cs
- backend/src/PerfectFit.UseCases/Leaderboard/GetLeaderboardQuery.cs
- backend/src/PerfectFit.UseCases/Leaderboard/GetLeaderboardQueryHandler.cs
- backend/src/PerfectFit.UseCases/Leaderboard/LeaderboardEntryResult.cs
- backend/src/PerfectFit.Web/Endpoints/AuthEndpoints.cs
- backend/src/PerfectFit.Web/Endpoints/LeaderboardEndpoints.cs

**Functions created/changed:**
- `LeaderboardEntryDto` - added `Avatar` field
- `UserDto` - added `Username` and `Avatar` fields
- `LeaderboardEntryResult` - added `Avatar` field
- `GetTopScoresQueryHandler.Handle()` - uses Username instead of DisplayName
- `GetUserStatsQueryHandler.Handle()` - uses Username instead of DisplayName
- `PUT /api/auth/profile` endpoint - new authenticated endpoint for profile updates
- `UpdateProfileRequest` - request DTO for profile updates
- `UpdateProfileResponse` - response DTO with success status and profile data
- `ProfileDto` - profile data returned in responses

**Tests created/changed:**
- All 301 unit tests passing (no regressions)

**Review Status:** APPROVED

**Git Commit Message:**
```
feat: add profile update endpoint and avatar to leaderboard

- Add PUT /api/auth/profile endpoint for username/avatar updates
- Add Avatar field to LeaderboardEntryDto and UserDto
- Update leaderboard queries to use Username instead of DisplayName
- Add UpdateProfileRequest/Response DTOs
- Ensure all user mappings include Username and Avatar fields
```

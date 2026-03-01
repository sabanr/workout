# Changelog

All notable changes to this project will be documented in this file.

## [1.8.0] - 2026-03-01

### Added
- New rolling 30-day dashboard metrics: Sessions, Volume, Avg Duration, Favorite Routine, and Consistency.
- Individual visibility settings for all dashboard metric cards.

### Changed
- Dashboard metrics now use a rolling 30-day window instead of current calendar month for better continuity.
- Updated dashboard layout to use consistent 200px cards for all stats.

### Fixed
- Dashboard metrics showing zero at the start of a new month.
- Loading issues for volume and duration calculations in dashboard.

## [1.7.0] - 2026-02-15

### Added
- Ability to cancel an ongoing workout session

### Fixed
- Language change not applying on macOS
- Language toggle not changing language dynamically

### Changed
- Globalized UI with EN/ES-AR resources and language toggle

## [1.6.0] - 2025-10-20

### Fixed
- Audio not playing when timer reaches 0 on Windows

### Changed
- Persist window size and position between sessions

## [1.5.1] - 2025-08-15

### Fixed
- App version display showing incorrect version

## [1.5.0] - 2025-07-10

### Added
- Dashboard metrics customization
- Play sound when timer reaches 0

### Fixed
- Exercise completed style
- Days streak calculation
- Start workout button height
- Graph data display

### Changed
- Show whole date in History page

## [1.4.0] - 2025-05-20

### Added
- Weight toggle to show weights in lbs and kg

### Fixed
- Y-axis label overflow in Weekly Volume chart

## [1.3.0] - 2025-03-15

### Fixed
- Weight unit labels from kg to lbs

## [1.2.0] - 2025-02-10

### Changed
- Removed iOS and Android from supported platforms (macOS only)

## [1.1.0] - 2025-01-20

### Added
- Initial release with basic workout tracking
- Routine CRUD operations
- Exercise templates with weights and reps suggestions
- Timer functionality
- Weekly volume chart
- History page
- Dashboard with workout streaks

### Fixed
- Various initial bug fixes

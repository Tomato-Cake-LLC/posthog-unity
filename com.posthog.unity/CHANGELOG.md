# Changelog

## 0.1.0

### Minor Changes

- **Event Capture**: Capture custom events with properties
- **Screen Tracking**: Track screen/scene views
- **User Identification**: Identify users with `IdentifyAsync` and reset with `ResetAsync`
- **Groups**: Associate users with companies/teams for group analytics
- **Super Properties**: Register properties sent with every event
- **Feature Flags**: Full support for feature flags with variants and payloads
- **Error Tracking**: Automatic capture of unhandled exceptions with stack traces
- **Application Lifecycle Events**: Automatic capture of install, update, open, and background events
- **Session Management**: Automatic session tracking
- **Opt-Out/Opt-In**: GDPR-compliant tracking controls
- **ScriptableObject Configuration**: Configure PostHog via Unity Inspector with `PostHogSettings` asset
- **Storage**: File-based persistence (PlayerPrefs fallback for WebGL)
- **Async Operations**: Non-blocking file writes and network requests
- **Platform Support**: Windows, macOS, Linux, iOS, Android, WebGL

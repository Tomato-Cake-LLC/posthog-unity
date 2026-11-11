using System.Collections.Generic;
using PostHogUnity;

namespace PostHogUnity.Tests
{
    public class SessionManagerTests
    {
        // In-memory IStorageProvider: SessionManager only ever touches the state methods.
        class MemoryStorage : IStorageProvider
        {
            readonly Dictionary<string, string> _state = new();

            public void Initialize(string basePath) { }

            public void SaveEvent(string id, string jsonData) { }

            public string LoadEvent(string id) => null;

            public void DeleteEvent(string id) { }

            public IReadOnlyList<string> GetEventIds() => new List<string>();

            public int GetEventCount() => 0;

            public void Clear() => _state.Clear();

            public void SaveState(string key, string jsonData) => _state[key] = jsonData;

            public string LoadState(string key) =>
                _state.TryGetValue(key, out var value) ? value : null;

            public void DeleteState(string key) => _state.Remove(key);
        }

        static string UuidV7At(DateTime timestampUtc)
        {
            var milliseconds = new DateTimeOffset(timestampUtc).ToUnixTimeMilliseconds();
            return $"{milliseconds:x12}".Insert(8, "-") + "-7ccf-bf3c-7c11f0adf456";
        }

        public class TheInitialSessionId
        {
            [Fact]
            public void IsUsedInsteadOfStartingANewSession()
            {
                var joined = UuidV7At(DateTime.UtcNow.AddMinutes(-5));

                var manager = new SessionManager(
                    new MemoryStorage(),
                    persistSession: false,
                    initialSessionId: joined
                );

                Assert.Equal(joined, manager.SessionId);
            }

            [Fact]
            public void SurvivesActivityWithinTheIdleTimeout()
            {
                var joined = UuidV7At(DateTime.UtcNow.AddMinutes(-45));
                var manager = new SessionManager(
                    new MemoryStorage(),
                    persistSession: false,
                    initialSessionId: joined
                );

                manager.Touch();

                // The joined session started 45 minutes ago, but this app just became active in it,
                // so the 30-minute idle timeout must not rotate it away.
                Assert.Equal(joined, manager.SessionId);
            }

            [Fact]
            public void RotatesWhenTheJoinedSessionIsPastItsMaximumLength()
            {
                var expired = UuidV7At(DateTime.UtcNow.AddHours(-25));
                var manager = new SessionManager(
                    new MemoryStorage(),
                    persistSession: false,
                    initialSessionId: expired
                );

                manager.Touch();

                // Past 24h PostHog drops the events from the session, so a fresh id is the only
                // useful outcome — proving the start time came from the id, not from the join.
                Assert.NotEqual(expired, manager.SessionId);
            }

            [Theory]
            [InlineData("550e8400-e29b-41d4-a716-446655440000")] // v4: not accepted by PostHog
            [InlineData("not-a-uuid")]
            public void IsRejectedUnlessItIsAUuidV7(string invalid)
            {
                // The rejection logs through UnityEngine.Debug, which throws under the Unity stubs.
                PostHogLogger.SetLogLevel(PostHogLogLevel.None);

                var manager = new SessionManager(
                    new MemoryStorage(),
                    persistSession: false,
                    initialSessionId: invalid
                );

                Assert.NotEqual(invalid, manager.SessionId);
                Assert.True(UuidV7.TryGetTimestamp(manager.SessionId, out _));
            }
        }

        public class TheDefaultBehaviour
        {
            [Fact]
            public void StartsItsOwnSessionWhenNoInitialSessionIdIsGiven()
            {
                var manager = new SessionManager(new MemoryStorage(), persistSession: false);

                Assert.True(UuidV7.TryGetTimestamp(manager.SessionId, out _));
            }
        }
    }
}

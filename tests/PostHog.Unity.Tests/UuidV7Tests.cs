using PostHogUnity;

namespace PostHogUnity.Tests
{
    public class UuidV7Tests
    {
        public class TheGenerateMethod
        {
            [Fact]
            public void ReturnsValidUuidFormat()
            {
                var uuid = UuidV7.Generate();

                Assert.NotNull(uuid);
                Assert.Equal(36, uuid.Length);
                Assert.True(Guid.TryParse(uuid, out _), "UUID should be parseable as a GUID");
            }

            [Fact]
            public void ReturnsVersion7Uuid()
            {
                var uuid = UuidV7.Generate();

                // Version 7 UUIDs have '7' as the first character of the third group
                // Format: xxxxxxxx-xxxx-7xxx-xxxx-xxxxxxxxxxxx
                var parts = uuid.Split('-');
                Assert.Equal(5, parts.Length);
                Assert.StartsWith("7", parts[2]);
            }

            [Fact]
            public void ReturnsVariant1Uuid()
            {
                var uuid = UuidV7.Generate();

                // Variant 1 UUIDs have the first character of the fourth group as 8, 9, a, or b
                var parts = uuid.Split('-');
                var variantChar = parts[3][0];
                Assert.True(
                    variantChar is '8' or '9' or 'a' or 'b',
                    $"UUID variant should be 8, 9, a, or b, got: {variantChar}"
                );
            }

            [Fact]
            public void ReturnsUniqueValues()
            {
                var uuids = new HashSet<string>();

                for (int i = 0; i < 1000; i++)
                {
                    var uuid = UuidV7.Generate();
                    Assert.True(uuids.Add(uuid), $"Duplicate UUID generated: {uuid}");
                }
            }

            [Fact]
            public void ReturnsTimeSortableUuids()
            {
                var uuids = new List<string>();

                for (int i = 0; i < 100; i++)
                {
                    uuids.Add(UuidV7.Generate());
                }

                // UUIDs should already be in sorted order since they're time-based
                var sorted = uuids.OrderBy(u => u).ToList();
                Assert.Equal(uuids, sorted);
            }
        }

        public class TheTryGetTimestampMethod
        {
            [Fact]
            public void ReturnsTheGenerationTime()
            {
                var before = DateTime.UtcNow.AddMilliseconds(-1);
                var uuid = UuidV7.Generate();
                var after = DateTime.UtcNow.AddMilliseconds(1);

                Assert.True(UuidV7.TryGetTimestamp(uuid, out var timestamp));
                Assert.InRange(timestamp, before, after);
            }

            [Theory]
            [InlineData("550e8400-e29b-41d4-a716-446655440000")] // version 4
            [InlineData("not-a-uuid")]
            [InlineData("")]
            public void RejectsAnythingThatIsNotAUuidV7(string value)
            {
                Assert.False(UuidV7.TryGetTimestamp(value, out var timestamp));
                Assert.Equal(default, timestamp);
            }

            // A well-formed v7 can carry any 48-bit value, and the top of that range is past the
            // year DateTime stops at. This arrives from a deep link, so it must not throw.
            [Fact]
            public void RejectsATimestampBeyondWhatADateTimeCanHold()
            {
                Assert.False(UuidV7.TryGetTimestamp("ffffffff-ffff-7fff-8fff-ffffffffffff", out _));
            }
        }

        public class TheGenerateBytesMethod
        {
            [Fact]
            public void Returns16Bytes()
            {
                var bytes = UuidV7.GenerateBytes();

                Assert.NotNull(bytes);
                Assert.Equal(16, bytes.Length);
            }

            [Fact]
            public void HasCorrectVersionBits()
            {
                var bytes = UuidV7.GenerateBytes();

                // Byte 6 should have version 7 in high nibble (0111xxxx = 0x7x)
                var versionNibble = (bytes[6] & 0xF0) >> 4;
                Assert.Equal(7, versionNibble);
            }

            [Fact]
            public void HasCorrectVariantBits()
            {
                var bytes = UuidV7.GenerateBytes();

                // Byte 8 should have variant 1 in high bits (10xxxxxx = 0x80-0xBF)
                var variantBits = (bytes[8] & 0xC0) >> 6;
                Assert.Equal(2, variantBits);
            }
        }
    }
}

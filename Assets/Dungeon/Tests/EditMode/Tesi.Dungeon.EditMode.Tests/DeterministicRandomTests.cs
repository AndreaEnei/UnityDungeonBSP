using System;
using NUnit.Framework;

namespace Tesi.Dungeon.Tests
{
    public sealed class DeterministicRandomTests
    {
        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var first = new DeterministicRandom(12345);
            var second = new DeterministicRandom(12345);

            for (int i = 0; i < 32; i++)
            {
                Assert.That(second.NextUInt(), Is.EqualTo(first.NextUInt()));
            }
        }

        [Test]
        public void SeedZero_DoesNotRemainInZeroState()
        {
            var random = new DeterministicRandom(0);

            for (int i = 0; i < 32; i++)
            {
                Assert.That(random.NextUInt(), Is.Not.EqualTo(0u));
            }
        }

        [Test]
        public void NextUInt_KnownSeed_MatchesVersionOneVector()
        {
            uint[] expected = 
            {
                3336926330u,
                1697253807u,
                2816511904u,
                1955480042u,
                718842323u
            };

            var random = new DeterministicRandom(12345);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(
                    random.NextUInt(),
                    Is.EqualTo(expected[i]),
                    $"Unexpected value at position {i}.");
            }
        }

        [Test]
        public void NextInt_ReturnsValuesInsideHalfOpenInterval()
        {
            var random = new DeterministicRandom(12345);

            const int minInclusive = -3;
            const int maxExclusive = 7;

            // Controlliamo un campione deterministico di 512 risultati come test di regressione.
            for (int i = 0; i < 512; i++)
            {
                int value = random.NextInt(minInclusive, maxExclusive);

                Assert.That(value, Is.GreaterThanOrEqualTo(minInclusive));
                Assert.That(value, Is.LessThan(maxExclusive));
            }
        }

        [TestCase(5, 5)]
        [TestCase(6, 5)]
        public void NextInt_InvalidInterval_Throws(
            int minInclusive,
            int maxExclusive)
        {
            var random = new DeterministicRandom(12345);

            ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => random.NextInt(minInclusive, maxExclusive)
            );
            /*
            // Posso usare in NUnit TestDelegate come tipo per un'operazione di test senza parametri o valore restituito.
            TestDelegate operation = () =>
            {
                random.NextInt(minInclusive, maxExclusive);
            };

            Assert.Throws<ArgumentOutOfRangeException>(operation);
            */

            Assert.That(exception.ParamName, Is.EqualTo("maxExclusive"));
        }
    }
}
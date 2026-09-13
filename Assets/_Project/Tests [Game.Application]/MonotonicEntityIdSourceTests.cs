using System;
using System.Threading.Tasks;
using Game.Application.Identity;
using NUnit.Framework;

[TestFixture]
public class MonotonicEntityIdSourceTests
{
    [Test]
    public void Allocate_ReturnsPositiveIncreasingIdentifiers()
    {
        MonotonicEntityIdSource source = new();

        int first = source.Allocate().Value;
        int second = source.Allocate().Value;
        int third = source.Allocate().Value;

        Assert.That(first, Is.EqualTo(1));
        Assert.That(second, Is.EqualTo(2));
        Assert.That(third, Is.EqualTo(3));
    }

    [Test]
    public void Allocate_AtRangeEnd_DoesNotWrapOrReuseIdentifiers()
    {
        MonotonicEntityIdSource source = new(int.MaxValue - 1);

        Assert.That(source.Allocate().Value, Is.EqualTo(int.MaxValue - 1));
        Assert.That(source.Allocate().Value, Is.EqualTo(int.MaxValue));
        Assert.Throws<InvalidOperationException>(() => source.Allocate());
        Assert.Throws<InvalidOperationException>(() => source.Allocate());
    }

    [Test]
    public void Allocate_Concurrently_ReturnsUniqueIdentifiers()
    {
        const int Count = 512;

        MonotonicEntityIdSource source = new();
        int[] allocated = new int[Count];

        Parallel.For(0, Count,
            index =>
            {
                allocated[index] = source.Allocate().Value;
            });

        Array.Sort(allocated);

        for (int index = 0; index < allocated.Length; index++)
        {
            Assert.That(allocated[index], Is.EqualTo(index + 1));
        }
    }
}

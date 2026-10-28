using System;
using System.Collections.Generic;
using Game.Arena.Presentation.Hud;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Game.Arena.PlayMode.Tests
{
    /// <summary>
    /// ADR-0035 / ADR-0033 metric 3: a presenter that re-publishes an unchanged
    /// value must not build a string or write a TMP text per frame. These tests
    /// exercise the real <see cref="HudView"/> in PlayMode, where
    /// <see cref="GC.GetAllocatedBytesForCurrentThread"/> works.
    /// </summary>
    [TestFixture]
    public sealed class HudAllocationTests
    {
        private const int MeasuredCalls = 100;

        private readonly List<GameObject> _objects = new();
        private HudView _hudView;
        private TextMeshProUGUI _healthText;
        private TextMeshProUGUI _equippedWeaponText;
        private TextMeshProUGUI _waveText;
        private TextMeshProUGUI _bossLabel;
        private TextMeshProUGUI _bossHealthText;

        [SetUp]
        public void SetUp()
        {
            _objects.Clear();

            GameObject root = Track(new GameObject("Hud"));
            _hudView = root.AddComponent<HudView>();
            _equippedWeaponText = root.AddComponent<TextMeshProUGUI>();

            _healthText = TrackText(root, "HealthText");
            _waveText = TrackText(root, "WaveText");
            _bossLabel = TrackText(root, "BossLabel");
            _bossHealthText = TrackText(root, "BossHealthText");

            GameObject healthFill = Track(new GameObject("HealthFill"));
            healthFill.transform.SetParent(root.transform, false);

            GameObject bossFill = Track(new GameObject("BossFill"));
            bossFill.transform.SetParent(root.transform, false);

            GameObject bossRoot = Track(new GameObject("BossRoot"));
            bossRoot.transform.SetParent(root.transform, false);

            _hudView.HealthTextMesh = _healthText;
            _hudView.WaveTextMesh = _waveText;
            _hudView.BossLabelMesh = _bossLabel;
            _hudView.BossHealthTextMesh = _bossHealthText;
            _hudView.HealthFill = healthFill.transform;
            _hudView.BossFill = bossFill.transform;
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        [Test]
        public void RepeatedHealthPresentation_WithUnchangedValues_AllocatesNothing()
        {
            _hudView.ShowHealth("Health", "100 / 100", 100, 100, 1f, false);

            long allocated = Measure(() =>
                _hudView.ShowHealth("Health", "100 / 100", 100, 100, 1f, false));

            Assert.That(allocated, Is.EqualTo(0));
        }

        [Test]
        public void RepeatedEquippedWeaponPresentation_WithUnchangedValues_AllocatesNothing()
        {
            _hudView.ShowEquippedWeapon("Equipped weapon", "Firearm");

            long allocated = Measure(() =>
                _hudView.ShowEquippedWeapon("Equipped weapon", "Firearm"));

            Assert.That(allocated, Is.EqualTo(0));
        }

        [Test]
        public void RepeatedWavePresentation_WithUnchangedValues_AllocatesNothing()
        {
            _hudView.ShowWave("Wave", "Wave 1 of 1");

            long allocated = Measure(() => _hudView.ShowWave("Wave", "Wave 1 of 1"));

            Assert.That(allocated, Is.EqualTo(0));
        }

        [Test]
        public void RepeatedBossPresentation_WithUnchangedValues_AllocatesNothing()
        {
            _hudView.ShowBoss("Boss", "500 / 500", 500, 500, 1f);

            long allocated = Measure(() =>
                _hudView.ShowBoss("Boss", "500 / 500", 500, 500, 1f));

            Assert.That(allocated, Is.EqualTo(0));
        }

        [Test]
        public void RepeatedBossHide_AllocatesNothing()
        {
            _hudView.ShowBoss("Boss", "500 / 500", 500, 500, 1f);
            _hudView.HideBoss();

            long allocated = Measure(() => _hudView.HideBoss());

            Assert.That(allocated, Is.EqualTo(0));
        }

        private static long Measure(Action action)
        {
            // One untimed call so a first-use cache inside TMP cannot be
            // attributed to the measured loop.
            action();

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int index = 0; index < MeasuredCalls; index++)
            {
                action();
            }

            return GC.GetAllocatedBytesForCurrentThread() - before;
        }

        private TextMeshProUGUI TrackText(GameObject root, string name)
        {
            GameObject text = Track(new GameObject(name));
            text.transform.SetParent(root.transform, false);

            return text.AddComponent<TextMeshProUGUI>();
        }

        private GameObject Track(GameObject created)
        {
            _objects.Add(created);

            return created;
        }
    }
}

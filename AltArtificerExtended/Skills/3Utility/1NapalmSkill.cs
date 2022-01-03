﻿using System;
using EntityStates;
using RoR2;
using RoR2.Skills;
using BepInEx.Configuration;
using AltArtificerExtended.Unlocks;
using UnityEngine;
using AltArtificerExtended.EntityState;
using RoR2.Projectile;
using ThreeEyedGames;
using R2API;
using R2API.Utils;

namespace AltArtificerExtended.Skills
{
    class _1NapalmSkill : SkillBase
    {
        //napalm
        public static GameObject projectilePrefabNapalm;
        public static GameObject acidPrefabNapalm;
        public static GameObject projectileNapalmImpact;
        public static GameObject projectileNapalmFX;

        public static float napalmDotFireFrequency = 1f;
        public static int napalmMaxProjectiles = ChargeNapalm.maxProjectileCount * ChargeNapalm.maxRowCount;
        public static float napalmBurnDPS =(ChargeNapalm.napalmBurnDamageCoefficient * ChargeNapalm.maxDamageCoefficient * napalmDotFireFrequency) / napalmMaxProjectiles;
        public override string SkillName => "Napalm Cascade";

        public override string SkillDescription => $"Charge up a barrage of fiery napalm, creating flaming pools that " +
            $"<style=cIsUtility>continuously burn</style> enemies " +
            $"for <style=cIsDamage>{napalmMaxProjectiles}x{Tools.ConvertDecimal(napalmBurnDPS)} damage per second</style>. " +
            $"Charging focuses the cone of fire.";

        public override string SkillLangTokenName => "NAPALM";

        public override UnlockableDef UnlockDef => GetUnlockDef(typeof(ArtificerNapalmUnlock));

        public override string IconName => "napalmicon";

        public override MageElement Element => MageElement.Fire;

        public override Type ActivationState => typeof(ChargeNapalm);

        public override SkillFamily SkillSlot => Main.mageUtility;

        public override SimpleSkillData SkillData => new SimpleSkillData
            (
                baseRechargeInterval: Main.artiUtilCooldown,
                interruptPriority: InterruptPriority.Skill,
                beginSkillCooldownOnSkillEnd: true
            );


        public override void Hooks()
        {
        }

        public override void Init(ConfigFile config)
        {
            RegisterProjectileNapalm();
            CreateLang();
            CreateSkill();
        }
        private void RegisterProjectileNapalm()
        {
            projectilePrefabNapalm = Resources.Load<GameObject>("prefabs/projectiles/beetlequeenspit").InstantiateClone("NapalmSpit", true);
            acidPrefabNapalm = Resources.Load<GameObject>("prefabs/projectiles/beetlequeenacid").InstantiateClone("NapalmFire", true);

            Color napalmColor = new Color32(255, 40, 0, 255);


            Transform pDotObjDecal = acidPrefabNapalm.transform.Find("FX/Decal");
            Material napalmDecalMaterial = new Material(pDotObjDecal.GetComponent<Decal>().Material);
            napalmDecalMaterial.SetColor("_Color", napalmColor);
            pDotObjDecal.GetComponent<Decal>().Material = napalmDecalMaterial;

            GameObject ghostPrefab = projectilePrefabNapalm.GetComponent<ProjectileController>().ghostPrefab;
            projectileNapalmFX = ghostPrefab.InstantiateClone("NapalmSpitGhost", false);
            Tools.GetParticle(projectileNapalmFX, "SpitCore", napalmColor);

            projectileNapalmImpact = Resources.Load<GameObject>("prefabs/effects/impacteffects/BeetleSpitExplosion").InstantiateClone("NapalmSpitExplosion", false);
            Tools.GetParticle(projectileNapalmImpact, "Bugs", Color.clear);
            Tools.GetParticle(projectileNapalmImpact, "Flames", napalmColor);
            Tools.GetParticle(projectileNapalmImpact, "Flash", Color.yellow);
            Tools.GetParticle(projectileNapalmImpact, "Distortion", napalmColor);
            Tools.GetParticle(projectileNapalmImpact, "Ring, Mesh", Color.yellow);

            ProjectileImpactExplosion pieNapalm = projectilePrefabNapalm.GetComponent<ProjectileImpactExplosion>();
            pieNapalm.childrenProjectilePrefab = acidPrefabNapalm;
            projectilePrefabNapalm.GetComponent<ProjectileController>().ghostPrefab = projectileNapalmFX;
            pieNapalm.impactEffect = projectileNapalmImpact;
            //projectilePrefabNapalm.GetComponent<ProjectileImpactExplosion>().destroyOnEnemy = true;
            pieNapalm.blastProcCoefficient = 0.6f;
            pieNapalm.bonusBlastForce = new Vector3(0, 500, 0);

            ProjectileDamage pd = projectilePrefabNapalm.GetComponent<ProjectileDamage>();
            pd.damageType = DamageType.IgniteOnHit;

            ProjectileDotZone pdz = acidPrefabNapalm.GetComponent<ProjectileDotZone>();
            pdz.lifetime = 8f;
            pdz.fireFrequency = napalmDotFireFrequency;
            pdz.damageCoefficient = ChargeNapalm.napalmBurnDamageCoefficient;
            pdz.overlapProcCoefficient = 0.3f;
            pdz.attackerFiltering = AttackerFiltering.Default;
            acidPrefabNapalm.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;
            acidPrefabNapalm.GetComponent<ProjectileController>().procCoefficient = 1f;

            float decalScale = 3.5f;
            acidPrefabNapalm.GetComponent<Transform>().localScale = new Vector3(decalScale, decalScale, decalScale);

            Transform transform = acidPrefabNapalm.transform.Find("FX");
            transform.Find("Spittle").gameObject.SetActive(false);

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(
                Resources.Load<GameObject>("prefabs/FireTrail").GetComponent<DamageTrail>().segmentPrefab, transform.transform);
            ParticleSystem.MainModule main = gameObject.GetComponent<ParticleSystem>().main;
            main.duration = 8f;
            main.gravityModifier = -0.075f;
            ParticleSystem.MinMaxCurve startSizeX = main.startSizeX;
            startSizeX.constantMin *= 0.6f;
            startSizeX.constantMax *= 0.8f;
            ParticleSystem.MinMaxCurve startSizeY = main.startSizeY;
            startSizeY.constantMin *= 0.8f;
            startSizeY.constantMax *= 1f;
            ParticleSystem.MinMaxCurve startSizeZ = main.startSizeZ;
            startSizeZ.constantMin *= 0.6f;
            startSizeZ.constantMax *= 0.8f;
            ParticleSystem.MinMaxCurve startLifetime = main.startLifetime;
            startLifetime.constantMin = 0.9f;
            startLifetime.constantMax = 1.1f;
            gameObject.GetComponent<DestroyOnTimer>().enabled = false;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;
            ParticleSystem.ShapeModule shape = gameObject.GetComponent<ParticleSystem>().shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.scale = Vector3.one * 0.5f;

            GameObject gameObject2 = transform.Find("Point Light").gameObject;
            Light component2 = gameObject2.GetComponent<Light>();
            component2.color = new Color(1f, 1f, 0f);
            component2.intensity = 4f;
            component2.range = 7.5f;

            Main.CreateEffect(projectileNapalmImpact);
            ContentPacks.projectilePrefabs.Add(projectilePrefabNapalm);
            ContentPacks.projectilePrefabs.Add(acidPrefabNapalm);
        }
    }
}
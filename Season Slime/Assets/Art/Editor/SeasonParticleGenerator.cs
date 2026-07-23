using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Generates the four season ambient particle prefabs (spring petals, summer
/// heat haze, autumn leaves, winter snow) together with their textures and
/// materials. Runs automatically once after script compilation if the prefabs
/// are missing; can also be re-run from the menu:
/// Tools > Season Slime > Regenerate Season Particle Prefabs.
/// </summary>
public static class SeasonParticleGenerator
{
    private const string TextureDir = "Assets/Art/Textures";
    private const string MaterialDir = "Assets/Art/Materials";
    private const string VfxDir = "Assets/Art/VFX";

    // Bump this to force automatic regeneration after script changes.
    private const int GeneratorVersion = 2;
    private const string VersionFilePath = VfxDir + "/.generator-version";

    private static readonly string[] PrefabPaths =
    {
        VfxDir + "/PS_SpringPetals.prefab",
        VfxDir + "/PS_SummerHeat.prefab",
        VfxDir + "/PS_AutumnLeaves.prefab",
        VfxDir + "/PS_WinterSnow.prefab",
    };

    [DidReloadScripts]
    private static void AutoGenerate()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            bool upToDate = File.Exists(VersionFilePath)
                && File.ReadAllText(VersionFilePath).Trim() == GeneratorVersion.ToString();

            foreach (string path in PrefabPaths)
            {
                if (!File.Exists(path))
                {
                    upToDate = false;
                }
            }

            if (!upToDate)
            {
                Generate();
            }
        };
    }

    [MenuItem("Tools/Season Slime/Regenerate Season Particle Prefabs")]
    private static void Generate()
    {
        Directory.CreateDirectory(TextureDir);
        Directory.CreateDirectory(MaterialDir);
        Directory.CreateDirectory(VfxDir);
        AssetDatabase.Refresh();

        Texture2D softCircle = CreateTexture(TextureDir + "/T_SoftCircle.png", SoftCircleAlpha);
        Texture2D petal = CreateTexture(TextureDir + "/T_Petal.png", PetalAlpha);
        Texture2D leaf = CreateTexture(TextureDir + "/T_Leaf.png", LeafAlpha);

        Material petalMat = CreateSpriteMaterial(MaterialDir + "/ParticlePetal.mat", petal);
        Material leafMat = CreateSpriteMaterial(MaterialDir + "/ParticleLeaf.mat", leaf);
        Material snowMat = CreateSpriteMaterial(MaterialDir + "/ParticleSnow.mat", softCircle);
        Material heatMat = CreateAdditiveMaterial(MaterialDir + "/ParticleHeat.mat", softCircle);

        CreateSpringPetals(petalMat);
        CreateSummerHeat(heatMat);
        CreateAutumnLeaves(leafMat);
        CreateWinterSnow(snowMat);

        AssetDatabase.SaveAssets();
        File.WriteAllText(VersionFilePath, GeneratorVersion.ToString());
        Debug.Log("[SeasonParticleGenerator] Season particle prefabs generated in " + VfxDir);
    }

    // ---------------------------------------------------------------- textures

    private static float SoftCircleAlpha(float x, float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        return Mathf.Pow(Mathf.Clamp01(1f - r), 1.8f);
    }

    private static float PetalAlpha(float x, float y)
    {
        // Narrow soft-edged ellipse.
        float v = (x / 0.5f) * (x / 0.5f) + (y / 0.85f) * (y / 0.85f);
        return Mathf.Clamp01((1f - v) / 0.15f);
    }

    private static float LeafAlpha(float x, float y)
    {
        // Pointed oval (vesica): intersection of two circles, tips on the Y axis.
        const float radius = 1.15f;
        const float offset = 0.6f;
        float d1 = Mathf.Sqrt((x - offset) * (x - offset) + y * y);
        float d2 = Mathf.Sqrt((x + offset) * (x + offset) + y * y);
        return Mathf.Clamp01((radius - Mathf.Max(d1, d2)) / 0.06f);
    }

    private static Texture2D CreateTexture(string path, System.Func<float, float, float> alphaFunc)
    {
        if (!File.Exists(path))
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f;
                    float v = (y + 0.5f) / size * 2f - 1f;
                    byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alphaFunc(u, v)) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(pixels);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // ---------------------------------------------------------------- materials

    private static Material CreateSpriteMaterial(string path, Texture2D texture)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.mainTexture = texture;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material CreateAdditiveMaterial(string path, Texture2D texture)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetTexture("_BaseMap", texture);
        mat.SetFloat("_Surface", 1f);   // transparent
        mat.SetFloat("_Blend", 2f);     // additive
        mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)BlendMode.One);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // ---------------------------------------------------------------- prefabs

    private static ParticleSystem NewSystem(string name, out ParticleSystemRenderer renderer)
    {
        var go = new GameObject(name);
        var ps = go.AddComponent<ParticleSystem>();
        renderer = go.GetComponent<ParticleSystemRenderer>();
        return ps;
    }

    private static void SavePrefab(ParticleSystem ps, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(ps.gameObject, path);
        Object.DestroyImmediate(ps.gameObject);
    }

    /// <summary>Looping, prewarmed system emitting from a wide box.</summary>
    private static void ConfigureAmbient(ParticleSystem ps, float width)
    {
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.prewarm = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 500;
        main.startSpeed = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(width, 1f, 1f);
    }

    private static ParticleSystem.MinMaxGradient FadeInOutGradient()
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f),
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.1f),
                new GradientAlphaKey(1f, 0.85f),
                new GradientAlphaKey(0f, 1f),
            });
        return new ParticleSystem.MinMaxGradient(gradient);
    }

    private static void CreateSpringPetals(Material mat)
    {
        ParticleSystem ps = NewSystem("PS_SpringPetals", out ParticleSystemRenderer renderer);
        ConfigureAmbient(ps, 26f);

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.72f, 0.85f), new Color(1f, 0.88f, 0.93f));
        main.gravityModifier = 0.02f;

        var emission = ps.emission;
        emission.rateOverTime = 8f;

        // NOTE: x, y and z must all use the same MinMaxCurve mode.
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-1.5f, -0.5f);
        velocity.y = new ParticleSystem.MinMaxCurve(-1.2f, -0.5f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var rotation = ps.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.6f;
        noise.frequency = 0.35f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = FadeInOutGradient();

        renderer.sharedMaterial = mat;

        SavePrefab(ps, VfxDir + "/PS_SpringPetals.prefab");
    }

    private static void CreateSummerHeat(Material mat)
    {
        ParticleSystem ps = NewSystem("PS_SummerHeat", out ParticleSystemRenderer renderer);
        ConfigureAmbient(ps, 26f);

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.9f, 0.65f, 0.1f), new Color(1f, 0.8f, 0.5f, 0.22f));

        var emission = ps.emission;
        emission.rateOverTime = 12f;

        // NOTE: x, y and z must all use the same MinMaxCurve mode.
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.5f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = FadeInOutGradient();

        renderer.sharedMaterial = mat;
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2.5f;
        renderer.maxParticleSize = 1.5f;

        SavePrefab(ps, VfxDir + "/PS_SummerHeat.prefab");
    }

    private static void CreateAutumnLeaves(Material mat)
    {
        ParticleSystem ps = NewSystem("PS_AutumnLeaves", out ParticleSystemRenderer renderer);
        ConfigureAmbient(ps, 26f);

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.4f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.95f, 0.55f, 0.15f), new Color(0.88f, 0.75f, 0.2f));
        main.gravityModifier = 0.04f;

        var emission = ps.emission;
        emission.rateOverTime = 6f;

        // NOTE: x, y and z must all use the same MinMaxCurve mode.
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-1.8f, -0.8f);
        velocity.y = new ParticleSystem.MinMaxCurve(-1.3f, -0.6f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var rotation = ps.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-2f, 2f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.8f;
        noise.frequency = 0.4f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = FadeInOutGradient();

        renderer.sharedMaterial = mat;

        SavePrefab(ps, VfxDir + "/PS_AutumnLeaves.prefab");
    }

    private static void CreateWinterSnow(Material mat)
    {
        ParticleSystem ps = NewSystem("PS_WinterSnow", out ParticleSystemRenderer renderer);
        ConfigureAmbient(ps, 26f);

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 14f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 0.9f), new Color(0.85f, 0.92f, 1f, 0.75f));

        var emission = ps.emission;
        emission.rateOverTime = 30f;

        // NOTE: x, y and z must all use the same MinMaxCurve mode.
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.6f, -0.2f);
        velocity.y = new ParticleSystem.MinMaxCurve(-1.2f, -0.6f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.25f;
        noise.frequency = 0.2f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = FadeInOutGradient();

        renderer.sharedMaterial = mat;

        SavePrefab(ps, VfxDir + "/PS_WinterSnow.prefab");
    }
}

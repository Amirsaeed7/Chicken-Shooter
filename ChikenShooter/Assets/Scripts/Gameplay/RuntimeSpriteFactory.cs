using System;
using UnityEngine;

public static class RuntimeSpriteFactory
{
    private static Sprite spaceship;
    private static Sprite asteroid;
    private static Sprite heart;
    private static Sprite bomb;
    private static Sprite standardChicken;
    private static Sprite armoredChicken;
    private static Sprite cyberChicken;
    private static Sprite bossChicken;
    private static Sprite egg;
    private static Sprite food;
    private static Sprite weaponGift;
    private static Sprite explosion;

    public static Sprite CreateCircle(Color color)
    {
        const int size = 32;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= radius ? color : new Color(0f, 0f, 0f, 0f);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateSpaceship()
    {
        if (spaceship != null) return spaceship;

        Vector2[] leftWing =
        {
            new Vector2(-0.15f, -0.22f), new Vector2(-0.82f, -0.58f),
            new Vector2(-0.55f, 0.12f), new Vector2(-0.28f, 0.3f)
        };
        Vector2[] leftWingInset =
        {
            new Vector2(-0.19f, -0.22f), new Vector2(-0.69f, -0.48f),
            new Vector2(-0.5f, 0.02f), new Vector2(-0.3f, 0.19f)
        };
        Vector2[] body =
        {
            new Vector2(0f, -0.88f), new Vector2(-0.36f, -0.22f),
            new Vector2(-0.27f, 0.42f), new Vector2(-0.14f, 0.78f),
            new Vector2(0f, 0.98f), new Vector2(0.14f, 0.78f),
            new Vector2(0.27f, 0.42f), new Vector2(0.36f, -0.22f)
        };
        Vector2[] bodyInset =
        {
            new Vector2(0f, -0.7f), new Vector2(-0.25f, -0.19f),
            new Vector2(-0.19f, 0.4f), new Vector2(-0.1f, 0.72f),
            new Vector2(0f, 0.87f), new Vector2(0.1f, 0.72f),
            new Vector2(0.19f, 0.4f), new Vector2(0.25f, -0.19f)
        };

        spaceship = CreateShapeSprite(64, point =>
        {
            if (point.y < -0.56f && Mathf.Abs(point.x) < (-point.y - 0.56f) * 0.38f)
            {
                if (point.y < -0.78f && Mathf.Abs(point.x) < (-point.y - 0.78f) * 0.22f)
                    return new Color(1f, 0.88f, 0.3f);
                return new Color(1f, 0.36f, 0.12f);
            }

            Vector2 mirrored = new Vector2(-point.x, point.y);
            if (ContainsPoint(point, leftWing) || ContainsPoint(mirrored, leftWing))
            {
                if (ContainsPoint(point, leftWingInset) || ContainsPoint(mirrored, leftWingInset))
                    return new Color(0.16f, 0.7f, 0.9f);
                return new Color(0.035f, 0.12f, 0.24f);
            }

            if (!ContainsPoint(point, body)) return Color.clear;
            if (!ContainsPoint(point, bodyInset)) return new Color(0.12f, 0.25f, 0.42f);

            float cockpit = point.x * point.x / 0.0225f + (point.y - 0.33f) * (point.y - 0.33f) / 0.0676f;
            if (cockpit < 0.7f) return new Color(0.66f, 0.96f, 1f);
            if (cockpit < 1f) return new Color(0.04f, 0.26f, 0.42f);
            if (point.x < -0.06f && point.y < 0.05f) return new Color(0.46f, 0.79f, 0.93f);
            return new Color(0.86f, 0.95f, 1f);
        });
        return spaceship;
    }

    public static Sprite CreateAsteroid()
    {
        if (asteroid != null) return asteroid;

        Vector2[] outline =
        {
            new Vector2(-0.68f, -0.12f), new Vector2(-0.58f, -0.58f),
            new Vector2(-0.19f, -0.83f), new Vector2(0.18f, -0.78f),
            new Vector2(0.62f, -0.55f), new Vector2(0.81f, -0.17f),
            new Vector2(0.69f, 0.32f), new Vector2(0.39f, 0.69f),
            new Vector2(-0.08f, 0.8f), new Vector2(-0.48f, 0.61f),
            new Vector2(-0.79f, 0.28f)
        };
        Vector2[] inset = new Vector2[outline.Length];
        for (int i = 0; i < outline.Length; i++)
            inset[i] = outline[i] * 0.84f;

        asteroid = CreateShapeSprite(64, point =>
        {
            if (!ContainsPoint(point, outline)) return Color.clear;
            if (!ContainsPoint(point, inset)) return new Color(0.22f, 0.19f, 0.2f);

            float craterA = (point.x + 0.28f) * (point.x + 0.28f) + (point.y - 0.2f) * (point.y - 0.2f);
            float craterB = (point.x - 0.32f) * (point.x - 0.32f) + (point.y + 0.25f) * (point.y + 0.25f);
            if (craterA < 0.035f || craterB < 0.025f) return new Color(0.25f, 0.22f, 0.22f);
            if (craterA < 0.05f || craterB < 0.038f) return new Color(0.68f, 0.59f, 0.5f);

            float variation = Mathf.Sin(point.x * 17f + point.y * 11f);
            return variation > 0.45f
                ? new Color(0.65f, 0.55f, 0.46f)
                : new Color(0.48f, 0.4f, 0.36f);
        });
        return asteroid;
    }

    public static Sprite CreateHeart()
    {
        if (heart != null) return heart;
        heart = CreateShapeSprite(64, point =>
        {
            float x = point.x * 1.15f;
            float y = point.y * 1.15f;
            float shape = Mathf.Pow(x * x + y * y - 1f, 3f) - x * x * y * y * y;
            if (shape > 0f) return Color.clear;

            float innerX = x * 1.12f;
            float innerY = y * 1.12f;
            float inner = Mathf.Pow(innerX * innerX + innerY * innerY - 1f, 3f) -
                          innerX * innerX * innerY * innerY * innerY;
            if (inner > 0f) return new Color(0.34f, 0.025f, 0.09f);
            if (point.x < -0.1f && point.y > 0.05f) return new Color(1f, 0.65f, 0.68f);
            return new Color(1f, 0.2f, 0.34f);
        });
        return heart;
    }

    public static Sprite CreateBomb()
    {
        if (bomb != null) return bomb;
        bomb = CreateShapeSprite(64, point =>
        {
            Vector2 center = new Vector2(-0.08f, -0.12f);
            float radius = Vector2.Distance(point, center);
            if (point.y > 0.38f && point.y < 0.68f &&
                Mathf.Abs(point.x - (point.y - 0.38f) * 0.55f) < 0.075f)
                return new Color(1f, 0.58f, 0.12f);
            if (radius > 0.62f) return Color.clear;
            if (radius > 0.52f) return new Color(0.78f, 0.9f, 1f);
            if (point.x < -0.2f && point.y > -0.05f) return new Color(0.55f, 0.77f, 0.9f);
            return new Color(0.12f, 0.22f, 0.36f);
        });
        return bomb;
    }

    public static Sprite CreateChicken(EnemyArchetype archetype)
    {
        switch (archetype)
        {
            case EnemyArchetype.Armored:
                if (armoredChicken == null)
                    armoredChicken = CreateChickenSprite(new Color(0.42f, 0.68f, 0.88f), new Color(0.2f, 0.42f, 0.64f), new Color(0.78f, 0.9f, 1f), true, false);
                return armoredChicken;
            case EnemyArchetype.Cyber:
                if (cyberChicken == null)
                    cyberChicken = CreateChickenSprite(new Color(0.66f, 0.3f, 0.78f), new Color(0.34f, 0.13f, 0.52f), new Color(0.35f, 1f, 0.9f), false, true);
                return cyberChicken;
            case EnemyArchetype.Asteroid:
                return CreateAsteroid();
            default:
                if (standardChicken == null)
                    standardChicken = CreateChickenSprite(new Color(1f, 0.72f, 0.28f), new Color(0.82f, 0.43f, 0.12f), new Color(1f, 0.91f, 0.62f), false, false);
                return standardChicken;
        }
    }

    public static Sprite CreateBoss()
    {
        if (bossChicken == null)
            bossChicken = CreateChickenSprite(new Color(0.75f, 0.25f, 0.28f), new Color(0.26f, 0.22f, 0.48f), new Color(1f, 0.79f, 0.22f), true, false, 96);
        return bossChicken;
    }

    public static Sprite CreateEgg()
    {
        if (egg != null) return egg;
        egg = CreateShapeSprite(48, point =>
        {
            float outerX = point.x / 0.48f;
            float outerY = (point.y + 0.02f) / 0.7f;
            if (outerX * outerX + outerY * outerY > 1f) return Color.clear;
            if (point.x * point.x / 0.13f + (point.y - 0.16f) * (point.y - 0.16f) / 0.25f < 1f)
                return new Color(1f, 0.84f, 0.35f);
            return new Color(0.96f, 0.9f, 0.7f);
        });
        return egg;
    }

    public static Sprite CreateFood()
    {
        if (food != null) return food;
        var wing = new[]
        {
            new Vector2(-0.88f, -0.02f), new Vector2(-0.78f, -0.34f),
            new Vector2(-0.52f, -0.58f), new Vector2(-0.25f, -0.62f),
            new Vector2(-0.06f, -0.4f), new Vector2(0.1f, -0.22f),
            new Vector2(0.36f, -0.16f), new Vector2(0.57f, 0.04f),
            new Vector2(0.56f, 0.31f), new Vector2(0.35f, 0.53f),
            new Vector2(0.1f, 0.49f), new Vector2(-0.11f, 0.27f),
            new Vector2(-0.37f, 0.3f), new Vector2(-0.63f, 0.2f)
        };
        var wingInset = new Vector2[wing.Length];
        for (int i = 0; i < wing.Length; i++)
            wingInset[i] = wing[i] * 0.84f;

        var bone = new[]
        {
            new Vector2(0.38f, 0.22f), new Vector2(0.68f, 0.48f),
            new Vector2(0.61f, 0.57f), new Vector2(0.3f, 0.31f)
        };
        food = CreateShapeSprite(48, point =>
        {
            bool boneKnob = InEllipse(point, 0.73f, 0.59f, 0.13f, 0.12f) ||
                            InEllipse(point, 0.83f, 0.43f, 0.12f, 0.12f);
            if (boneKnob || ContainsPoint(point, bone))
                return point.x < 0.64f ? new Color(0.73f, 0.53f, 0.3f) : new Color(1f, 0.93f, 0.72f);

            if (!ContainsPoint(point, wing)) return Color.clear;
            if (!ContainsPoint(point, wingInset)) return new Color(0.31f, 0.13f, 0.045f);
            if (InEllipse(point, -0.34f, -0.12f, 0.065f, 0.09f) ||
                InEllipse(point, -0.02f, 0.05f, 0.055f, 0.07f) ||
                InEllipse(point, 0.23f, 0.24f, 0.05f, 0.07f))
                return new Color(0.67f, 0.3f, 0.08f);
            if (point.x < -0.35f && point.y > -0.04f) return new Color(1f, 0.78f, 0.34f);
            return new Color(0.91f, 0.48f, 0.13f);
        });
        return food;
    }

    public static Sprite CreateWeaponGift()
    {
        if (weaponGift != null) return weaponGift;
        weaponGift = CreateShapeSprite(48, point =>
        {
            if (Mathf.Abs(point.x) > 0.68f || Mathf.Abs(point.y) > 0.58f)
            {
                if (Mathf.Abs(point.x) < 0.08f && Mathf.Abs(point.y) < 0.78f)
                    return new Color(1f, 0.85f, 0.28f);
                if (Mathf.Abs(point.y) < 0.09f && Mathf.Abs(point.x) < 0.78f)
                    return new Color(1f, 0.85f, 0.28f);
                return Color.clear;
            }

            if (Mathf.Abs(point.x) > 0.52f || Mathf.Abs(point.y) > 0.44f)
                return new Color(0.12f, 0.2f, 0.4f);
            if (Mathf.Abs(point.x) < 0.11f || Mathf.Abs(point.y) < 0.12f)
                return new Color(0.75f, 0.97f, 1f);
            return new Color(0.18f, 0.68f, 0.9f);
        });
        return weaponGift;
    }

    public static Sprite CreateExplosion()
    {
        if (explosion != null) return explosion;
        explosion = CreateShapeSprite(96, point =>
        {
            float distance = point.magnitude;
            float angle = Mathf.Atan2(point.y, point.x);
            float edge = 0.7f + 0.23f * Mathf.Cos(angle * 12f);
            if (distance > edge) return Color.clear;
            if (distance < 0.25f) return new Color(1f, 1f, 0.78f);
            if (distance < 0.48f) return new Color(1f, 0.82f, 0.16f);
            return new Color(1f, 0.28f, 0.06f);
        });
        return explosion;
    }

    private static Sprite CreateChickenSprite(Color feathers, Color wings, Color detail, bool armored, bool cyber, int size = 64)
    {
        var wing = new Vector2[]
        {
            new Vector2(-0.2f, -0.16f), new Vector2(-0.72f, -0.45f),
            new Vector2(-0.62f, 0.18f), new Vector2(-0.34f, 0.39f)
        };
        var wingInset = new Vector2[]
        {
            new Vector2(-0.22f, -0.1f), new Vector2(-0.62f, -0.34f),
            new Vector2(-0.55f, 0.12f), new Vector2(-0.34f, 0.3f)
        };
        var beak = new Vector2[]
        {
            new Vector2(-0.17f, 0.05f), new Vector2(0.17f, 0.05f),
            new Vector2(0f, -0.2f)
        };
        var beakInset = new Vector2[]
        {
            new Vector2(-0.1f, 0.025f), new Vector2(0.1f, 0.025f),
            new Vector2(0f, -0.13f)
        };

        return CreateShapeSprite(size, point =>
        {
            Vector2 mirrored = new Vector2(-point.x, point.y);
            if (ContainsPoint(point, beak) || ContainsPoint(mirrored, beak))
                return ContainsPoint(point, beakInset) || ContainsPoint(mirrored, beakInset)
                    ? new Color(1f, 0.67f, 0.08f)
                    : new Color(0.35f, 0.14f, 0.04f);

            if (point.y < -0.55f && point.y > -0.82f && Mathf.Abs(point.x) < 0.035f)
                return new Color(0.35f, 0.14f, 0.04f);
            if (point.y < -0.73f && point.y > -0.82f && Mathf.Abs(point.x) < 0.15f)
                return new Color(1f, 0.58f, 0.08f);

            if (InEllipse(point, -0.1f, 0.64f, 0.11f, 0.16f) ||
                InEllipse(point, 0.1f, 0.64f, 0.11f, 0.16f) ||
                InEllipse(point, 0f, 0.72f, 0.13f, 0.18f))
                return new Color(0.82f, 0.08f, 0.12f);

            if (InEllipse(point, -0.15f, 0.27f, 0.09f, 0.12f) ||
                InEllipse(point, 0.15f, 0.27f, 0.09f, 0.12f))
                return new Color(1f, 0.97f, 0.82f);
            if (InEllipse(point, -0.13f, 0.25f, 0.035f, 0.07f) ||
                InEllipse(point, 0.17f, 0.25f, 0.035f, 0.07f))
                return cyber ? detail : new Color(0.12f, 0.08f, 0.08f);

            if (ContainsPoint(point, wing) || ContainsPoint(mirrored, wing))
            {
                if (ContainsPoint(point, wingInset) || ContainsPoint(mirrored, wingInset))
                    return wings;
                return new Color(0.2f, 0.11f, 0.08f);
            }

            bool body = InEllipse(point, 0f, -0.1f, 0.48f, 0.54f);
            bool head = InEllipse(point, 0f, 0.28f, 0.34f, 0.34f);
            if (body || head)
            {
                bool innerBody = InEllipse(point, 0f, -0.08f, 0.42f, 0.48f);
                bool innerHead = InEllipse(point, 0f, 0.29f, 0.29f, 0.29f);
                if ((!innerBody && body) || (!innerHead && head))
                    return new Color(0.2f, 0.11f, 0.08f);

                if (armored && point.y > 0.37f)
                    return point.y > 0.56f ? new Color(0.15f, 0.2f, 0.32f) : detail;
                if (cyber && Mathf.Abs(point.x) < 0.045f && point.y < -0.14f)
                    return detail;
                if (point.x < -0.12f && point.y > -0.3f)
                    return detail;
                return feathers;
            }

            if (armored && point.y > 0.82f && Mathf.Abs(point.x) < 0.24f)
                return detail;

            return Color.clear;
        });
    }

    private static bool InEllipse(Vector2 point, float centerX, float centerY, float radiusX, float radiusY)
    {
        float x = (point.x - centerX) / radiusX;
        float y = (point.y - centerY) / radiusY;
        return x * x + y * y <= 1f;
    }

    private static Sprite CreateShapeSprite(int size, Func<Vector2, Color> colorAt)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var point = new Vector2(x / (size - 1f) * 2f - 1f, y / (size - 1f) * 2f - 1f);
                pixels[y * size + x] = colorAt(point);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static bool ContainsPoint(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                (polygon[j].y - polygon[i].y) + polygon[i].x)
                inside = !inside;
        }
        return inside;
    }
}

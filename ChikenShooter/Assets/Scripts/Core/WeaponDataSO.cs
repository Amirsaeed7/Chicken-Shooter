using UnityEngine;

[CreateAssetMenu(menuName = "Chicken Shooter/Weapon Data", fileName = "WeaponData")]
public class WeaponDataSO : ScriptableObject
{
    public string weaponName = "Laser";
    public Sprite sprite;
    public Projectile projectilePrefab;
    public float damage = 1f;
    public float projectileSpeed = 12f;
    public float fireRate = 0.18f;
}

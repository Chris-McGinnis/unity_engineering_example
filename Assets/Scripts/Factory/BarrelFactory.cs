using UnityEngine;

[CreateAssetMenu(fileName = "BarrelFactory", menuName = "McG/Factories/Barrel Factory")]
public class BarrelFactory : ScriptableObject
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private string instanceName = "Explosive Barrel";

    [Header("Barrel Configuration")] [SerializeField, Min(1f)]
    private float maxHealth = 30f;

    [SerializeField, Min(0f)] private float fuseDuration = 2f;

    public ExplosiveBarrel CreateBarrel(Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError($"[Factory] Prefab missing on {name}!", this);
            return null;
        }

        GameObject instance = Instantiate(prefab, position, rotation);
        instance.name = instanceName;
        ExplosiveBarrel barrel = instance.GetComponent<ExplosiveBarrel>();
        barrel.Configure(maxHealth, fuseDuration);
        return barrel;
    }
}

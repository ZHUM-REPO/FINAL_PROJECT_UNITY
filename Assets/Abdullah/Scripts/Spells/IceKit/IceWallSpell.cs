using UnityEngine;

public class IceWallSpell : SpellBase
{
    [Header("Ice Wall Settings")]
    public GameObject iceWallPrefab;
    public float wallDuration = 10f;
    public float maxPlaceDistance = 15f;
    public LayerMask placementMask; // surfaces the wall can be placed on

    [HideInInspector] public bool upgradeLongerDuration = false;    // level 1
    [HideInInspector] public bool upgradeWiderWall = false;         // level 2
    [HideInInspector] public bool upgradeFreezeOnTouch = false;     // level 3

    protected override void Cast()
    {
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2));

        if (!Physics.Raycast(ray, out RaycastHit hit, maxPlaceDistance, placementMask))
        {
            Debug.Log("No valid surface to place Ice Wall.");
            return;
        }

        PlayCastParticles();

        // align wall to the surface normal
        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        Quaternion modelOffset = Quaternion.Euler(-180f, 0f, 0f);
        Quaternion wallRotation = surfaceRotation * modelOffset;
        GameObject wall = Instantiate(iceWallPrefab, hit.point, wallRotation);

        IceWall iceWall = wall.GetComponent<IceWall>();
        if (iceWall != null)
        {
            iceWall.duration = upgradeLongerDuration ? wallDuration * 2f : wallDuration;
            iceWall.isWider = upgradeWiderWall;
            iceWall.freezeOnTouch = upgradeFreezeOnTouch;
        }
    }
}
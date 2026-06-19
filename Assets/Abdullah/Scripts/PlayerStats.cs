using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxEndurance = 100f;
    public float myEndurance;
    public float enduranceDrain = 15f;
    public float enduranceRegain = 10f;

    [Range(0f, 1f)]
    public float sprintResumeThreshold = 0.25f;
    public bool canSprint = true;

    void Start()
    {
        myEndurance = maxEndurance;
    }

    public void EnduranceDrain()
    {
        myEndurance -= enduranceDrain * Time.deltaTime;
        myEndurance = Mathf.Clamp(myEndurance, 0f, maxEndurance);

        if (myEndurance <= 0f)
        {
            canSprint = false;
        }
    }

    public void EnduranceRegain()
    {
        myEndurance += enduranceRegain * Time.deltaTime;
        myEndurance = Mathf.Clamp(myEndurance, 0f, maxEndurance);

        if (!canSprint && myEndurance >= maxEndurance * sprintResumeThreshold)
        {
            canSprint = true;
        }
    }
}

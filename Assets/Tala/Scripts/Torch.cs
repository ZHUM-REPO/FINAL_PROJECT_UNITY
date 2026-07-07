using UnityEngine;

public class Torch : MonoBehaviour
{
    [Header("Puzzle Reference")]
    public TorchPuzzleManager puzzleManager;

    [Header("Effects")]
    public ParticleSystem fireParticles;

    private bool isLit = false;
    public bool IsLit => isLit;

    void Start()
    {
        if (fireParticles != null)
        {
            fireParticles.Stop();
        }
    }

    public void LightTorch()
    {
        if (isLit) return;

        isLit = true;

        if (fireParticles != null)
        {
            fireParticles.Play();
        }

        if (puzzleManager != null)
        {
            puzzleManager.TorchActivated(this);
        }
    }

    // تعيد المشعل إلى حالته الأصلية وتطفئ النار فوراً عند الخطأ
    public void Extinguish()
    {
        isLit = false;
        if (fireParticles != null)
        {
            fireParticles.Stop();
        }
    }
}
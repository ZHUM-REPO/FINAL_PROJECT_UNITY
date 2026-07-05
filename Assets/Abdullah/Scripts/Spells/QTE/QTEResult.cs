public enum QTEGrade
{
    Perfect,
    Good,
    Partial,
    Fail
}

public class QTEResult
{
    public QTEGrade grade;
    public float effectMultiplier;
    public int correctInputs;
    public int totalInputs;

    public QTEResult(int correct, int total)
    {
        correctInputs = correct;
        totalInputs = total;

        float score = total > 0 ? (float)correct / total : 0f;

        if (score >= 0.90f)
        {
            grade = QTEGrade.Perfect;
            effectMultiplier = 1.0f;
        }
        else if (score >= 0.65f)
        {
            grade = QTEGrade.Good;
            effectMultiplier = 0.75f;
        }
        else if (score >= 0.35f)
        {
            grade = QTEGrade.Partial;
            effectMultiplier = 0.40f;
        }
        else
        {
            grade = QTEGrade.Fail;
            effectMultiplier = 0f;
        }
    }
}
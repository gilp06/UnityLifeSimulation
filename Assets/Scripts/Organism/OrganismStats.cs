public class OrganismStats
{
    public float fieldOfView;
    public float maxViewDistance;

    public float speedRatio;
    public float sizeRatio;

    public int offspringMutationCount;
    public float offspringStartingEnergy;
    public float offspringIncubateTime;
    public float offspringHatchTime;
    
    public float red;
    public float blue;
    public float green;

    public OrganismStats(float fieldOfView,
        float maxViewDistance,
        float speedRatio,
        float sizeRatio,
        int offspringMutationCount,
        float offspringStartingEnergy,
        float offspringIncubateTime,
        float offspringHatchTime,
        float red,
        float blue,
        float green)
    {
        this.fieldOfView = fieldOfView;
        this.maxViewDistance = maxViewDistance;
        this.speedRatio = speedRatio;
        this.sizeRatio = sizeRatio;
        this.offspringMutationCount = offspringMutationCount;
        this.offspringStartingEnergy = offspringStartingEnergy;
        this.offspringIncubateTime = offspringIncubateTime;
        this.offspringHatchTime = offspringHatchTime;
        this.red = red;
        this.blue = blue;
        this.green = green;
    }

    public OrganismStats(OrganismStats parentStats)
    {
        fieldOfView = parentStats.fieldOfView;
        maxViewDistance = parentStats.maxViewDistance;
        speedRatio = parentStats.speedRatio;
        sizeRatio = parentStats.sizeRatio;
        offspringMutationCount = parentStats.offspringMutationCount;
        offspringStartingEnergy = parentStats.offspringStartingEnergy;
        offspringIncubateTime = parentStats.offspringIncubateTime;
        offspringHatchTime = parentStats.offspringHatchTime;
        red = parentStats.red;
        green = parentStats.green;
        blue = parentStats.blue;
    }
}

public enum OrganismStatsMutationAction
{
    MutateFieldOfView,
    MutateMaxViewDistance,
    MutateSpeedRatio,
    MutateSizeRatio,
    MutateOffspringMutationCount,
    MutateOffspringStartingEnergy,
    MutateOffspringIncubateTime,
    MutateOffspringHatchTime,
    MutateColor,
}
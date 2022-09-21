using System;

public class StatsInfo
{
    public int Number { get; set; }
    public int Population { get; set; }
    public float MaxFitness { get; set; }
    public float MedianFitness { get; set; }
    public float PreviousMaxFitness { get; set; }
    public float PreviousMedianFitness { get; set; }
    public TimeSpan Duration { get; set; }
}

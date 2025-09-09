// File: PlotState.cs
public enum PlotState
{
    Untilled,   // default state, plain dirt
    Tilled,     // player tilled the soil
    Planted,    // something is planted
    Watered,    // tilled+planted and watered
}
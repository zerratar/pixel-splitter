using System;

namespace LiveSplit.PixelSplitter.Models
{
    [Flags]
    public enum GameImageMatchActionType
    {
        PauseOnMatch = 0b00000001,
        PausedWhileMatch = 0b00000010,
        ResumeOnMatch = 0b00000100,
        SplitOnMatch = 0b00001000,
        SplitAndPauseOnMatch = 0b00010000,
        SkipOnMatch = 0b00100000,
        EndOnMatch = 0b01000000,
        AbortOnMatch = 0b10000000,
        StartOnMatch = 0b100000000,
    }
}
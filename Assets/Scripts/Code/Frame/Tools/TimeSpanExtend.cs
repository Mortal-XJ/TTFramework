//----------------------
// Developer Mortal
// Date 2023 - - 
// Script Overview 
//----------------------

using System;
using UnityEngine;

namespace GameFrame
{
    public static class TimeSpanExtend
    {
        public static string ConvertMillisecondsToMinutesSeconds(this Time self ,int milliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
            return string.Format("{0}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);
        }
    }
}
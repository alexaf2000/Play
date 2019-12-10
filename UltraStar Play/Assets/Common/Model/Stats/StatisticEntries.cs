﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles storage and operation over the song statistic entries themselves
[Serializable]
public class StatisticEntries
{
    public SortedSet<SongStatistic> SongStatistics { get; private set; }

    public StatisticEntries()
    {
        SongStatistics = new SortedSet<SongStatistic>(new CompareBySongScore());
    }

    public void AddRecord(SongStatistic record)
    {
        SongStatistics.Add(record);
    }

    public void RemoveRecord(SongStatistic record)
    {
        SongStatistics.Remove(record);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MVS;
using UnityEngine;

internal struct MVSRunnerInitializeArgs
{
    public int? PlayerCount;

    public Action<MVSRunner> OnGameStarted;
}

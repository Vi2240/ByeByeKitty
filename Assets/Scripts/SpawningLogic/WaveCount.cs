using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

public class WaveCount : Singleton<WaveCount>
{
    [SerializeField] TextMeshProUGUI waveCountText = null;

    private static int countOfWaves;

    public void ModifyCountOfWaves()
    {
        countOfWaves++;
        ShowWaveCountText();
    }

    public void ResetCountOfWaves()
    {
        countOfWaves = 0;
        ShowWaveCountText();
    }

    private void ShowWaveCountText()
    {
        waveCountText.text = countOfWaves.ToString();
    }
}

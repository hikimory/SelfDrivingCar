using TMPro;
using UnityEngine;

public class UIPrinter : MonoBehaviour
{
	[SerializeField] private TMP_Text m_generationNumber;
	[SerializeField] private TMP_Text m_populationSize;
	[SerializeField] private TMP_Text m_maxFitness;
	[SerializeField] private TMP_Text m_medianFitness;
	[SerializeField] private TMP_Text m_maxFitnessDifference;
	[SerializeField] private TMP_Text m_medianFitnessDifference;
	[SerializeField] private TMP_Text _duration;
	
	private readonly Color s_Green = new Color(0f, 1f, 0f, 1f);
	private readonly Color s_Red = new Color(1f, 0f, 0f, 1f);

    private void Start()
    {
		InitInfoText();
	}

	private void InitInfoText()
    {
		m_generationNumber.text = "0";
		_duration.text = "00:00:00";
		m_populationSize.text = "0";
		m_maxFitness.text = string.Format("{0:0.0}", 0);
		m_medianFitness.text = string.Format("{0:0.0}", 0);
		m_maxFitnessDifference.text = string.Format("{0:0.0}", 0);
		m_medianFitnessDifference.text = string.Format("{0:0.0}", 0);
	}

    public void UpdateInfo(StatsInfo info)
	{
		m_generationNumber.text = info.Number.ToString();
		m_populationSize.text = info.Population.ToString();
		_duration.text = $"{info.Duration.Hours}:{info.Duration.Minutes}:{info.Duration.Seconds}";
		m_maxFitness.text = string.Format("{0:0.00}", info.MaxFitness);
		m_medianFitness.text = string.Format("{0:0.00}", info.MedianFitness);
		

		if ((info.PreviousMaxFitness - info.MaxFitness) <= 0)
		{
			m_maxFitnessDifference.color = s_Green;
			m_maxFitnessDifference.text = string.Format("+{0:0.0}", info.MaxFitness - info.PreviousMaxFitness);
		}
		else
		{
			m_maxFitnessDifference.color = s_Red;
			m_maxFitnessDifference.text = string.Format("-{0:0.0}", info.PreviousMaxFitness - info.MaxFitness);
		}

		if ((info.PreviousMedianFitness - info.MedianFitness) <= 0)
		{
			m_medianFitnessDifference.color = s_Green;
			m_medianFitnessDifference.text = string.Format("+{0:0.0}", info.MedianFitness - info.PreviousMedianFitness);
		}
		else
		{
			m_medianFitnessDifference.color = s_Red;
			m_medianFitnessDifference.text = string.Format("-{0:0.0}", info.PreviousMedianFitness - info.MedianFitness);
		}
	}
}


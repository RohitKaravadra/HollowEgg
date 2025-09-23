using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HealthBar
{
    [System.Serializable]
    struct HealthUI
    {
        public GameObject HealthEgg;
        public Image Full;
        public Image Broken;
    }

    [SerializeField] HealthUI[] _HealthBar;

    public void SetHealth(int health, int maxHealth)
    {
        for (int i = 0; i < _HealthBar.Length; i++)
        {
            if (i < maxHealth)
            {
                _HealthBar[i].HealthEgg.SetActive(true);
                if (i < health)
                {
                    _HealthBar[i].Full.enabled = true;
                    _HealthBar[i].Broken.enabled = false;
                }
                else
                {
                    _HealthBar[i].Full.enabled = false;
                    _HealthBar[i].Broken.enabled = true;
                }
            }
            else
            {
                _HealthBar[i].HealthEgg.SetActive(false);
                _HealthBar[i].Full.enabled = false;
                _HealthBar[i].Broken.enabled = false;
            }
        }
    }
}

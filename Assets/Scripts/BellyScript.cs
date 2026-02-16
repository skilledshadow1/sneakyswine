using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BellyCounter : MonoBehaviour
{
    [SerializeField] private Transform bellyBone;
    [SerializeField] private float startScale;
    [SerializeField] private float endScale;

    public TextMeshProUGUI bellyText; // Reference to the UI Text component
    private int currentCount = 0; // Current food count
    private int maxCount = 10; // Maximum food count
    
    // Start is called before the first frame update
    void Start()
    {
        bellyBone.localScale = new Vector3(startScale, startScale, startScale);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
    }
    
    public void IncreaseCount()
    {
        if (currentCount < maxCount) // Prevent exceeding max count
        {
            currentCount++;
        }
        else
        {
            if (bellyText != null)
            {
                bellyText.text = "Escape!";
            }
        }

        float scale = Mathf.Lerp(startScale, endScale, currentCount / maxCount);
        bellyBone.localScale = new Vector3(scale, scale, scale);
    }
    
    private void UpdateUI()
    {
        bellyText.text = $"Fullness: {currentCount} / {maxCount}";
    }
}
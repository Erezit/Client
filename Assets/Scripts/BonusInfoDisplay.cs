using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays bonus information in the shop UI
/// </summary>
public class BonusInfoDisplay : MonoBehaviour
{
    [Header("Bonus Info")]
    public TextMeshProUGUI bonusTitle;
    public TextMeshProUGUI bonusDescription;
    public TextMeshProUGUI bonusCost;
    
    public void SetBonusInfo(string title, string description, int cost)
    {
        if (bonusTitle != null) bonusTitle.text = title;
        if (bonusDescription != null) bonusDescription.text = description;
        if (bonusCost != null) bonusCost.text = $"Стоимость: {cost} золота";
    }
    
    // For setting up in Unity Editor
    public void ShowClickPowerInfo()
    {
        SetBonusInfo("Усилить Клик", "Увеличивает силу клика на 1.\nПостоянный эффект.", 50);
    }
    
    public void ShowNodeBoostInfo()
    {
        SetBonusInfo("Усилить Узел", "Увеличивает показатель вашего узла на 5.\nТребует выбора узла.", 30);
    }
    
    public void ShowGoldGenInfo()
    {
        SetBonusInfo("Ускорить Генерацию", "Увеличивает генерацию золота на 50%.\nПостоянный эффект.", 100);
    }
    
    public void ShowAttackInfo()
    {
        SetBonusInfo("Атаковать Врага", "Наносит 7 урона вражескому узлу.\nТребует выбора вражеского узла.", 40);
    }
}

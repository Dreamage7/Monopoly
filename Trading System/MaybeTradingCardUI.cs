using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaybeTradingCardUI : MonoBehaviour
{
    [SerializeField] Image colorField;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] Image iconImage;
    [SerializeField] GameObject mortgageImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;
    [SerializeField] Toggle chackToggle;
    MonopolyNode nodeRefernce;
    public MonopolyNode NodeRefernce => nodeRefernce;
    public void SetCard(MonopolyNode node,ToggleGroup toggleGroup)
    {
        nodeRefernce = node;
        if (node.propertyColoerField != null)
        {
            colorField.color = node.propertyColoerField.color;
        }
        propertyNameText.text = node.name;
        propertyPriceText.text = MaybeTradingSystem.instance.CalculateValueOfNode(node) + "$";
        switch (node.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                iconImage.sprite = houseSprite;
                iconImage.color = Color.green;
                break;
            case MonopolyNodeType.Railroad:
                iconImage.sprite = railroadSprite;
                iconImage.color = Color.white;
                break;
            case MonopolyNodeType.Utility:
                iconImage.sprite = utilitySprite;
                iconImage.color = Color.black;
                break;
        }
        mortgageImage.SetActive(node.IsMortgaged);
        chackToggle.isOn = false;
        chackToggle.group = toggleGroup;
    }
    public void SetCard(MonopolyNode node)
    {
        if (node.propertyColoerField != null)
        {
            colorField.color = node.propertyColoerField.color;
        }
        propertyNameText.text = node.name;
        propertyPriceText.text = MaybeTradingSystem.instance.CalculateValueOfNode(node) + "$";
        switch (node.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                iconImage.sprite = houseSprite;
                iconImage.color = Color.green;
                break;
            case MonopolyNodeType.Railroad:
                iconImage.sprite = railroadSprite;
                iconImage.color = Color.white;
                break;
            case MonopolyNodeType.Utility:
                iconImage.sprite = utilitySprite;
                iconImage.color = Color.black;
                break;
        }
        mortgageImage.SetActive(node.IsMortgaged);
    }
    public void ResetCard()
    {
        colorField.color = Color.black;
        propertyNameText.text = "Null";
        propertyPriceText.text = "";
        iconImage.sprite = houseSprite;
        iconImage.color = Color.white;
        mortgageImage.SetActive(false);
    }
}

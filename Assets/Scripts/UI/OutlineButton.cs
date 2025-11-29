using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OutlineButton : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    private Outline outline;
    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.enabled = false;
    }

    void Awake()
    {
        outline = this.GetOrAddComponent<Outline>();
        outline.effectColor = Color.red;
        outline.effectDistance = new Vector2(6, -6);
        outline.useGraphicAlpha = false;
        outline.enabled = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

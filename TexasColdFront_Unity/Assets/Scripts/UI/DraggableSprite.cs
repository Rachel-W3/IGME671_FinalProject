using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace tcf.ui
{

public enum DraggableType
{
    SNOWBALL = 0,
    FIREFUEL = 1,
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Image))]
public class DraggableSprite : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /**************/ public  Camera         cameraThatRenders;
    [SerializeField] private DraggableType  draggableType;
    [SerializeField] private float          velocityDivisor;
    /**************/ private RectTransform  containerThatRenders;
    /**************/ private Image          thisImg;
    /**************/ private Rigidbody2D    thisRB;
    /**************/ private RectTransform  thisRT;

    // Audio
    private FMOD.Studio.EventInstance snowballGrabbing_sfx;

    public DraggableType Type => draggableType;

    private void Start()
    {
        snowballGrabbing_sfx = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/SnowballGrabbing");
        thisRB = GetComponent<Rigidbody2D>();
        thisImg = GetComponent<Image>();
        thisRT = GetComponent<RectTransform>();
        containerThatRenders = thisRT.parent.GetComponent<RectTransform>();
        // TODO: set sprite based on draggable type, pulling from sprite singleton from a set of random valid sprites
    }

    private void Update()
    {
        if (thisRB.isKinematic)
            thisRT.anchoredPosition = Constants.MousePointToCanvasPoint(containerThatRenders);
        thisRB.velocity = Vector2.ClampMagnitude(thisRB.velocity, Screen.height / velocityDivisor);
    }

    public void OnPointerDown(PointerEventData _)
    {
        snowballGrabbing_sfx.start();
        thisRB.isKinematic = true;
        thisRB.gravityScale = 0;
    }

    public void OnPointerUp(PointerEventData _)
    {
        thisRB.isKinematic = false;
        thisRB.gravityScale = 100;
    }
}

}
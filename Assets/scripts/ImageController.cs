﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//UI图片拖拽功能类
public class ImageController : Singleton<ImageController>, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    protected ImageController() { }

    //存储图片中心点与鼠标点击点的偏移量
    private Vector3 m_Offset;
    private Vector3 m_GlobalMousePosition;

    //存储当前拖拽图片的RectTransform组件
    private RectTransform m_RT;
    private RectTransform m_RT_Parent;

    public bool HaveImage
    {
        get
        {
            return Texture.texture != null;
        }
    }

    [SerializeField]
    private float m_AdjustOnZooming = 2f;
    [SerializeField]
    private float m_MaxScale = 10.0f;
    [SerializeField]
    private  float m_MinScale = 0.8f;

    public RawImage Texture { get; set; }

    void Start()
    {
        m_RT = gameObject.GetComponent<RectTransform>();
        m_RT_Parent = transform.parent.GetComponent<RectTransform>();
        Texture = GetComponent<RawImage>();
    }

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && RectTransformUtility.RectangleContainsScreenPoint(m_RT_Parent, Input.mousePosition) && HaveImage && !ProjectStage.Instance.IsMainImageDragging)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_RT, Input.mousePosition, null, out m_GlobalMousePosition);
            Vector3 localScale = m_RT.localScale;
            float scaleFactor = Input.GetAxis("Mouse ScrollWheel") * m_AdjustOnZooming * localScale.x;
            Vector3 scale = new Vector3(localScale.x + scaleFactor, localScale.y + scaleFactor, 1);
            //scale.x = (scale.x > m_MaxScale) ? m_MaxScale : scale.x;
            //scale.y = (scale.y > m_MaxScale) ? m_MaxScale : scale.y;
            //scale.x = (scale.x < m_MinScale) ? m_MinScale : scale.x;
            //scale.y = (scale.y < m_MinScale) ? m_MinScale : scale.y;

            Vector3 delta = m_GlobalMousePosition - m_RT.position;
            Vector3 pos = new Vector3(delta.x * (scale.x - localScale.x) / localScale.x, delta.y * (scale.y - localScale.y) / localScale.y, 0);
            m_RT.position -= pos;

            m_RT.localScale = scale;
        }        
    }

    void OnGUI()
    {
        //if (RectTransformUtility.RectangleContainsScreenPoint(m_RT_Parent, Input.mousePosition) && HaveImage
        //    && Event.current.isMouse && Event.current.button == 1 && Event.current.clickCount == 2)
        //{
            
        //}
    }

    public void ViewFullImage()
    {
        m_RT.anchoredPosition = Vector2.zero;
        m_RT.localScale = Vector3.one;
    }

    //开始拖拽触发
    //When using a mouse the pointerId returns -1, -2, or -3. These are the left, right and center mouse buttons respectively.
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HaveImage || eventData.pointerId != -3)
            return;
        ProjectStage.Instance.IsMainImageDragging = true;

        // 存储点击时的鼠标坐标
        Vector3 tWorldPos;
        //UI屏幕坐标转换为世界坐标
        RectTransformUtility.ScreenPointToWorldPointInRectangle(m_RT, eventData.position, eventData.pressEventCamera, out tWorldPos);
        //计算偏移量   
        m_Offset = m_RT.position - tWorldPos;

        SetDraggedPosition(eventData);
    }

    //拖拽过程中触发
    public void OnDrag(PointerEventData eventData)
    {
        if (!HaveImage || eventData.pointerId != -3)
            return;
        SetDraggedPosition(eventData);
    }

    //结束拖拽触发
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!HaveImage || eventData.pointerId != -3)
            return;
        ProjectStage.Instance.IsMainImageDragging = false;
        SetDraggedPosition(eventData);
    }

    /// <summary>
    /// 设置图片位置方法
    /// </summary>
    /// <param name="eventData"></param>
    private void SetDraggedPosition(PointerEventData eventData)
    {
        //UI屏幕坐标转换为世界坐标
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_RT, eventData.position, eventData.pressEventCamera, out m_GlobalMousePosition))
        {
            //设置位置及偏移量
            m_RT.position = m_GlobalMousePosition + m_Offset;
        }
    }

    void OnDestroy()
    {
        if (HaveImage)
        {
            Destroy(Texture.texture);
            Texture.texture = null;
            Resources.UnloadUnusedAssets();
        }
    }

    public void ConfigureContentView()
    {
        Rect viewportRect = m_RT_Parent.GetComponent<RectTransform>().rect;
        float viewportAspect = viewportRect.width / viewportRect.height;
        float spriteAspect = (float)Texture.texture.width / Texture.texture.height;

        if (viewportAspect > spriteAspect)
        {
            m_RT.sizeDelta = new Vector2(viewportRect.height * spriteAspect, viewportRect.height);
        }
        else
        {
            m_RT.sizeDelta = new Vector2(viewportRect.width, viewportRect.width / spriteAspect);
        }
        m_RT.anchoredPosition = Vector2.zero;
        m_RT.localScale = Vector3.one;
    }
}
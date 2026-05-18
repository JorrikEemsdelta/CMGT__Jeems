using UnityEngine;

public class SmoothImageSlider : MonoBehaviour
{
    [Tooltip("The RectTransform that contains all your images.")]
    public RectTransform contentPanel;
    
    [Tooltip("How fast the slide animation plays.")]
    public float slideSpeed = 10f;
    
    [Tooltip("The exact width of one image, plus any spacing between them.")]
    public float stepDistance = 1080f; 

    private int currentIndex = 0;
    private int totalImages;
    private Vector2 targetPosition;

    void Start()
    {
        // Automatically count how many images are inside the panel
        totalImages = contentPanel.childCount;
        
        // Set the starting target to wherever the panel is currently placed
        targetPosition = contentPanel.anchoredPosition;
    }

    void Update()
    {
        // Smoothly interpolate the panel's position towards the target
        contentPanel.anchoredPosition = Vector2.Lerp(
            contentPanel.anchoredPosition, 
            targetPosition, 
            Time.deltaTime * slideSpeed
        );
    }

    public void Next()
    {
        if (currentIndex < totalImages - 1)
        {
            currentIndex++;
            UpdateTarget();
        }
    }

    public void Previous()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateTarget();
        }
    }

    private void UpdateTarget()
    {
        // Calculate the new X position based on the current index
        targetPosition = new Vector2(-currentIndex * stepDistance, contentPanel.anchoredPosition.y);
    }
}
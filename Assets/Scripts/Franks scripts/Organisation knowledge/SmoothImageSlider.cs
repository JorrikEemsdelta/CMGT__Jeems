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

    // This runs when the script starts. It counts the number of child images in the panel and sets the starting target position to its current coordinate.
    void Start()
    {
        // Automatically count how many images are inside the panel
        totalImages = contentPanel.childCount;
        
        // Set the starting target to wherever the panel is currently placed
        targetPosition = contentPanel.anchoredPosition;
    }

    // This runs every frame and smoothly Lerps (interpolates) the content panel position towards the active target position.
    void Update()
    {
        // Smoothly interpolate the panel's position towards the target
        contentPanel.anchoredPosition = Vector2.Lerp(
            contentPanel.anchoredPosition, 
            targetPosition, 
            Time.deltaTime * slideSpeed
        );
    }

    // This transitions to the next image in the list if we haven't reached the end, updating the target coordinates.
    public void Next()
    {
        if (currentIndex < totalImages - 1)
        {
            currentIndex++;
            UpdateTarget();
        }
    }

    // This transitions to the previous image in the list if we aren't at the first image, updating the target coordinates.
    public void Previous()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateTarget();
        }
    }

    // This calculates the horizontal X coordinate offset for the target position based on the current image index and slide step distance width.
    private void UpdateTarget()
    {
        // Calculate the new X position based on the current index
        targetPosition = new Vector2(-currentIndex * stepDistance, contentPanel.anchoredPosition.y);
    }
}
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform mask;
    [SerializeField] Vector2 padding = new(9,8);
    private RectTransform maskParentRectTransform;
    [SerializeField][Range(0, 1)] private float progress;

    private void Update()
    {
        SetProgress(progress);    
    }

    private void Awake()
    {
        if(mask == null)
        {
            Debug.LogError($"Progress bar {name} is missing a mask! THis progress bar will not work!");
            return;
        }

        maskParentRectTransform = mask.parent.GetComponent<RectTransform>();
    }

    public void SetProgress(float progress)
    {
        Vector2 parentSize = maskParentRectTransform.sizeDelta;
        Vector2 targetSize = parentSize - padding * 2;

        targetSize.x *= Mathf.Clamp01(progress);

        mask.offsetMin = padding;
        mask.offsetMax = new Vector2(padding.x + targetSize.x - parentSize.x, -padding.y);
    }
}

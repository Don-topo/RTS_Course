using System.Collections;
using UnityEngine;

public class BuildingBuildingUI : MonoBehaviour, IUIElement<BaseBuilding>
{
    [SerializeField] private UIBuildQueueButton[] unitButtons;
    [SerializeField] private ProgressBar progressBar;

    private Coroutine buildCoroutine;
    private BaseBuilding building;

    public void Disable()
    {
        if(building != null)
        {
            building.OnQueueUpdated -= HandleQueueUpdated;
        }

        building = null;
        gameObject.SetActive(false);
        buildCoroutine = null;
    }

    public void EnableFor(BaseBuilding item)
    {
        if(building != null)
        {
            building.OnQueueUpdated -= HandleQueueUpdated;
        }

        progressBar.SetProgress(0);
        building = item;
        gameObject.SetActive(true);
        building.OnQueueUpdated += HandleQueueUpdated;
        SetupButtons();

        buildCoroutine = StartCoroutine(UpdateUnitProgress());
    }

    private void SetupButtons()
    {
        int i = 0;
        for (; i < building.QueueSize; i++)
        {
            int index = i;
            unitButtons[i].EnableFor(building.Queue[i], () => building.CancelBuildingUnit(index));
        }

        for (; i < unitButtons.Length; i++)
        {
            unitButtons[i].Disable();
        }
    }

    private void HandleQueueUpdated(UnlockableSO[] unitsInQueue)
    {
        if(unitsInQueue.Length == 1 && buildCoroutine == null)
        {
            buildCoroutine = StartCoroutine(UpdateUnitProgress());
        }

        if(building != null)
        {
            SetupButtons();
        }        
    }

    private IEnumerator UpdateUnitProgress()
    {
        while(building != null && building.QueueSize > 0)
        {
            float startTime = building.CurrentQueueStartTime;
            float endTime = startTime + building.SOBeingBuild.BuildTime;

            float progress = Mathf.Clamp01((Time.time - startTime) / (endTime - startTime));

            progressBar.SetProgress(progress);
            yield return null;
        }
        buildCoroutine = null;
    }
}

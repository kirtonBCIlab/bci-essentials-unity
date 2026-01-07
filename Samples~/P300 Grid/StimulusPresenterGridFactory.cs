using BCIEssentials.Stimulus.Collections;
using BCIEssentials.Stimulus.Presentation;
using UnityEngine;

public class StimulusPresenterGridFactory : StimulusPresenterCollection
{
    [SerializeField] private StimulusPresentationBehaviour _presenterPrefab;
    [SerializeField] private int _rowCount = 3;
    [SerializeField] private int _columnCount = 3;
    [SerializeField] private Vector2 _spacing = Vector2.one;


    [ContextMenu("Build Presenters")]
    public void CreatePresenters() => CreatePresenters(transform);
    public void CreatePresenters(Transform objectParent)
    {
        if (_presenterPrefab == null)
        {
            Debug.LogWarning("No Presenter prefab for Factory");
            return;
        }

        if (OptionCount > 0) DestroyPresenters();

        BuildGridObjects(objectParent);
    }

    private void BuildGridObjects(Transform objectParent)
    {
        for (int rowIndex = 0; rowIndex < _rowCount; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < _columnCount; columnIndex++)
            {
                var presenter = InstantiatePresenter(objectParent);
                GameObject presenterObject = presenter.gameObject;

                presenterObject.name = $"Stimulus Presenter {OptionCount}";
                presenterObject.transform.position = new()
                {
                    x = GetPositionAxis(_spacing.x, columnIndex, _columnCount),
                    y = GetPositionAxis(_spacing.y, rowIndex, _rowCount)
                };
            }
        }
    }

    private float GetPositionAxis(float spacing, float index, float segmentCount)
    => spacing * (index - (segmentCount - 1) / 2);

    private StimulusPresentationBehaviour InstantiatePresenter(Transform parent)
    {
        StimulusPresentationBehaviour newPresenter
            = (parent == null)
            ? Instantiate(_presenterPrefab)
            : Instantiate(_presenterPrefab, parent);
        _stimulusPresenters.Add(newPresenter);
        return newPresenter;
    }


    [ContextMenu("Clean Up Presenters")]
    public void DestroyPresenters()
    {
        foreach (StimulusPresentationBehaviour presenter in _stimulusPresenters)
        {
            if (presenter != null)
            {
                if (!Application.isEditor) Destroy(presenter.gameObject);
                else DestroyImmediate(presenter.gameObject);
            }
        }
        _stimulusPresenters.Clear();
    }
}
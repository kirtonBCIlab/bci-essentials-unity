using UnityEditor;
using UnityEngine;


namespace BCIEssentials.Tests.Resources
{
    [CreateAssetMenu(menuName = "BCI Essentials/Testing/Resource Library", fileName = "ResourceLibrary", order = 0)]
    public class ResourceLibraryAsset : ScriptableObject
    {
        [SerializeField] private SceneAsset _emptyScene;
        public SceneAsset EmptyScene => _emptyScene;
        
        [SerializeField] private SceneAsset _defaultScene;
        public SceneAsset DefaultScene => _defaultScene;
    }
}
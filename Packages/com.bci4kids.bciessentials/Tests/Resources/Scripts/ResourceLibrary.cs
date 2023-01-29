using UnityEditor;

namespace BCIEssentials.Tests.TestResources
{
    public static class ResourceLibrary
    {
        private const string k_LibraryPath = "Assets/com.bci4kids.bciessentials/Tests/Resources/ResourceLibrary.asset";
        
        private static ResourceLibraryAsset _library;

        public static ResourceLibraryAsset Library
        {
            get
            {
                if (_library == null)
                {
                    LoadResourceLibraryAsset();
                }
                
                return _library;
            }
        }

        private static void LoadResourceLibraryAsset()
        {
            _library = AssetDatabase.LoadAssetAtPath<ResourceLibraryAsset>(k_LibraryPath);
        }
    }
}
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataBinding
{
    using System.Reflection;
    
    
    public class PackagePath
    {
        
        private const string LocalPath = "Assets/_package_/";
        
        public const string DisplayName = "Data Binding";
        
        private static string _mainPath;
        
        public static string MainPath
        {
            get
            {
                if (string.IsNullOrEmpty(_mainPath))
                {
                    var p = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetAssembly(typeof(PackagePath)));;
                    if (p == null)
                    {
                        _mainPath = LocalPath;
                    }
                    else
                    {
                        _mainPath = p.assetPath + "/";
                    }
                }
                return _mainPath;
            }
        }
    }
}

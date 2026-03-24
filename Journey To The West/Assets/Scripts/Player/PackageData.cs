using UnityEngine;

[CreateAssetMenu(fileName = "NewPackage", menuName = "Scriptable Objects/Package Data")]
public class PackageData : ScriptableObject
{
    public string packageName;
    public string sealDescription;
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    private List<ItemData> items = new List<ItemData>();
    private List<PackageData> packages = new List<PackageData>();
 
    public UnityEvent onInventoryChanged;
 
 
    public void AddItem(ItemData item)
    {
        items.Add(item);
        onInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemData item)
    {
        if (items.Remove(item))
        {
            onInventoryChanged?.Invoke();
        }
    }

    public bool HasItem(string name)
    {
        return items.Any(item => item.itemName == name);
    }

    public IReadOnlyList<ItemData> GetItems()
    {
        return items;
    }
    
    public void AddPackage(PackageData package)
    {
        packages.Add(package);
        onInventoryChanged?.Invoke();
    }
 
    public void RemovePackage(PackageData package)
    {
        if (packages.Remove(package))
        {
            onInventoryChanged?.Invoke();
        }
    }
 
    public int GetPackageCount()
    {
        return packages.Count;
    }
 
    public IReadOnlyList<PackageData> GetPackages()
    {
        return packages;
    }
}
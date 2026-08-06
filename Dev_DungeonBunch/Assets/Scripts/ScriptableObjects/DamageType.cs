using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageType", menuName = "Scriptable Objects/DamageType")]
public class DamageTypeSO : ScriptableObject
{
    [SerializeField] private string key;
    public string Key => this.name;
    /* [SerializeField] private List<string> tags;
    public HashSet<string> Tags; */

    void OnValidate()
    {
        key = this.name.ToUnderscoreCase().Trim();

        /* List<string> tagsRewrite = new List<string>();
        foreach (var entry in tags)
        {
            tagsRewrite.Add(entry.ToUnderscoreCase().Trim());
        }
        tags = tagsRewrite;

        Tags = new HashSet<string>(tags); */
    }
}

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Utility component for testing that runs <see cref="IRunnable"/> components every frame.
/// Used for controlling execution order between different components.  
/// </summary>
public class UpdateRunner : MonoBehaviour
{
    [SerializeField] List<MonoBehaviour> tickables = new();

    private void OnValidate()
    {
        var set = new HashSet<MonoBehaviour>();
        foreach (var mono in tickables)
        {
            if (mono is IRunnable)
            {
                set.Add(mono);
            }
        }
        tickables = set.ToList();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var tickable in tickables)
        {
            IRunnable t = (IRunnable)tickable;
            t.Run();
        }
    }
}

public interface IRunnable
{
    public void Run();
}

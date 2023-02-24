using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A piston board simulator that allows you to configure various properties such as the piston type, separation
/// distance, the dimension of pistons, colors, etc.
///
/// TODO:
/// - Change piston colors to reflect rain drop positions
/// </summary>
public class PistonBoard : MonoBehaviour
{
    [Tooltip("The dimensions of the piston board")] [SerializeField]
    private Vector2Int dimensions = new Vector2Int(3, 3);

    [Tooltip("The prefab of each piston to be created")] [SerializeField]
    private GameObject pistonPrefab;

    [Tooltip("The distance for each piston to actuate")] [SerializeField]
    private float pistonDistance;

    [Tooltip("The distance in between each of the pistons")] [SerializeField]
    private Vector2 spacing;
    
    public Vector2Int Dimensions
    {
        get => dimensions;
        set
        {
            dimensions = value;
            CreatePistons();
        }
    } 

    /// <summary>
    /// The parent that contains all of the pistons within.
    /// </summary>
    private GameObject _pistonsParent;

    /// <summary>
    /// Each of the pistons.
    /// </summary>
    private GameObject[,] _pistons;
    
    private void Awake()
    {
        CreatePistons();
    }

    private void OnValidate()
    {
        // Only modify piston board properties while in play mode.
        if (Application.isPlaying)
        {
            CreatePistons();     
        }
    }

    private void OnDestroy()
    {
        Destroy(_pistonsParent);
        _pistonsParent = null;
        _pistons = null;
    }

    // private void Update()
    // {
    //     // Don't try to animate while not in edit mode (it gets weird)
    //     if (!Application.isPlaying)
    //         return;
    //     
    //     // Create a sine wave effect
    //     float[,] distances = new float[dimensions.x, dimensions.y];
    //
    //     for (int x = 0; x < dimensions.x; x++)
    //     {
    //         for (int y = 0; y < dimensions.y; y++)
    //         {
    //             float distance = 0.5f * (Mathf.Sin(Time.time + x) + 1f);
    //             distances[x, y] = distance;
    //         }
    //     }
    //     
    //     SetPistonPositions(distances);
    // }

    /// <summary>
    /// Creates the pistons using the specified parameters above.
    /// </summary>
    private void CreatePistons()
    {
        // If no piston prefab exists, we cannot do anything.
        if (!pistonPrefab)
        {
            Debug.LogError("Cannot create piston board because there is no piston prefab!");
            return;
        }
        
        // If a previous piston board exists, destroy it.
        if (_pistonsParent)
        {
            Destroy(_pistonsParent);
        }

        // Create each of the pistons in the board.
        _pistonsParent = new GameObject("Piston Board");
        _pistonsParent.transform.SetParent(transform);
        
        // Center the piston board in the center of the parent.
        _pistonsParent.transform.localPosition =
            new Vector3((dimensions.x * spacing.x) / 2f, 0, (dimensions.y * spacing.y) / 2f);
        _pistonsParent.transform.localRotation = Quaternion.identity;
        
        _pistons = new GameObject[dimensions.x, dimensions.y];
        
        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                Vector3 pistonPosition = new Vector3(x * spacing.x, 0f, y * spacing.y);
                GameObject piston = Instantiate(pistonPrefab, pistonPosition,
                    Quaternion.identity, _pistonsParent.transform);
                
                _pistons[x, y] = piston;
            }
        }
    }

    public void SetPistonPosition(int x, int y, float distance)
    {
        // Ensure that a valid piston position was given.
        if (!(0 <= x && x < dimensions.x && 0 <= y && y < dimensions.y))
        {
            throw new ArgumentException("This is not a valid piston position!");
        }

        GameObject piston = _pistons[x, y];
        
        // Make sure the "distance" parameter is clamped between 0 and 1 (0 for fully retracted and 1 for fully
        // extended) and multiply it by the pistonDistance parameter to get its actuation distance.
        float newDistance = pistonDistance * Mathf.Clamp(distance, 0f, 1f);

        Vector3 pistonPos = piston.transform.localPosition;
        pistonPos.y = newDistance;
        piston.transform.localPosition = pistonPos;
    }

    public void SetPistonPositions(float[,] positions)
    {
        // Make sure that pistons are actually there.
        if (_pistons == null)
        {
            throw new ArgumentException("No pistons exist, so no positions can be set!");
        }
        
        // Ensure that the given piston positions match the dimensions of the pistons.
        // positions.GetLength(0): length of x dimension (aka rows)
        // positions.GetLength(1): length of y dimension (aka columns)
        if (positions.GetLength(0) != dimensions.x || positions.GetLength(1) != dimensions.y)
        {
            throw new ArgumentException("These positions do not match the dimensions of the pistons!");
        }
        
        // Apply piston positions
        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                SetPistonPosition(x, y, positions[x,y]);
            }
        }
    }
}
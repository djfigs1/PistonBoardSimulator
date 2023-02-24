using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simulates the ripples of rain drops and outputs them to a piston board. 
/// </summary>
public class RainDropEffect : MonoBehaviour
{
    [SerializeField] private PistonBoard board;
    [SerializeField] private float rippleRange = 0.5f;

    private List<Raindrop> _raindrops = new List<Raindrop>();

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Raindrop r = new Raindrop(new Vector2(0.1f, 0.1f), 10f, 5f);
            AddRaindrop(r);
        } else if (Input.GetKeyDown(KeyCode.R))
        {
            Raindrop r = new Raindrop(new Vector2(0.9f, 0.9f), 10f, 5f);
            AddRaindrop(r);
        }
    }

    public void AddRaindrop(Raindrop r)
    {
        _raindrops.Add(r);
    }

    private void FixedUpdate()
    {
        var dimensions = board.Dimensions;
        float[,] distances = new float[dimensions.x, dimensions.y];
        
        // Remove raindrops that are ready to be removed (they have exceeded their spread time).
        _raindrops.RemoveAll(raindrop => raindrop.ReadyToRemove);
        
        // Update pistons
        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                Vector2 point = new Vector2(x / (dimensions.x - 1f), y / (dimensions.y - 1f));

                float average = 0f;
                if (_raindrops.Count > 0)
                {
                    average = _raindrops.Select(r =>
                    {
                        float distance = r.GetDistanceFromRipple(point);
                        float pistonDistance = Mathf.Max(0f, -Mathf.Pow(distance / rippleRange, 2) + 1);

                        return pistonDistance;
                    }).Average();
                }
                
                distances[x, y] = average;
            }
        }
        
        board.SetPistonPositions(distances);
    }

    public class Raindrop
    {
        public Vector2 origin;
        public float radius;
        public float spreadTime;
        private float _spawnTime;
        
        // 0 - 1: 0 just created, 1 fully spread
        public float SpreadPercentage => (Time.time - _spawnTime) / spreadTime;

        public bool ReadyToRemove => Time.time >= _spawnTime + spreadTime;
        
        public Raindrop(Vector2 origin, float spreadTime, float radius)
        {
            this.origin = origin;
            this.spreadTime = spreadTime;
            this.radius = radius;
            _spawnTime = Time.time;
        }

        public float GetDistanceFromRipple(Vector2 point)
        {
            float currentRadius = radius * SpreadPercentage;
            return Vector2.Distance(point, origin) - currentRadius;
        }
    }
}

using System;
using UnityEngine;
using ZXC.Geometry;
using ZXC.RayTracing;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour
{
    private void Start()
    {
        var numSpheres = 2; 
        var spheres = new Sphere[numSpheres];
        for (var i = 0; i < numSpheres; ++i) { 
            var center = new Vector3(i * 2, i * 2, i * 2);
            var radius = 2f;
            // var color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            // var center = Vector3.zero;
            // var radius = 2f;
            var color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            spheres[i] = new Sphere(center, radius, color, 0, 0); 
        } 
        
        var rayTracer = new RayTracer();
        rayTracer.options = new Options
        {
            width = 512,
            height = 512,
            maxDepth = 5
        };
        rayTracer.camera = Camera.main;
        rayTracer.Render(spheres);
    }
}

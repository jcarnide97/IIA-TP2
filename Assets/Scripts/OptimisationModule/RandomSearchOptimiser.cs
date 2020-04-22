using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;


public class RandomSearchOptimiser : OptimisationAlgorithm
{
    private int bestCost;
    private List<int> newSolution = null;

    private string fileName = "Assets/Logs/" + System.DateTime.Now.ToString("ddhmmsstt") + "_RandomSearchOptimiser.csv";


    protected override void Begin()
    {

        CreateFile(fileName);
        bestSequenceFound = new List<GameObject>();

        // Initialization.
        this.newSolution = GenerateRandomSolution(targets.Count);
        int quality = Evaluate(newSolution);
        base.CurrentSolution = new List<int>(newSolution);
        bestCost = quality;

        //DO NOT CHANGE THE LINES BELLOW
        AddInfoToFile(fileName, CurrentNumberOfIterations, this.Evaluate(base.CurrentSolution), base.CurrentSolution);
        CurrentNumberOfIterations++;
    }

    protected override void Step()
    {
        
        this.newSolution = GenerateRandomSolution(targets.Count);
        int cost = Evaluate(newSolution);
        if (cost < bestCost)
        {
            base.CurrentSolution = new List<int>(newSolution);
            bestCost = cost;
        }

        //DO NOT CHANGE THE LINES BELLOW
        AddInfoToFile(fileName, CurrentNumberOfIterations, this.Evaluate(base.CurrentSolution), base.CurrentSolution);
        CurrentNumberOfIterations++;

    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;


public class SimulatedAnnealingOptimiser : OptimisationAlgorithm
{
    private List<int> newSolution = null;
    private int CurrentSolutionCost;
    public float TempInicial;
    public float Temperature;
    private float zero = Mathf.Pow(10, -6);// numbers bellow this value can be considered zero.

    string fileName = "Assets/Logs/" + System.DateTime.Now.ToString("ddhmmsstt") + "_SimulatedAnnealingOptimiser.csv";

    public float Diminuicao; // percentagem da diminuição de tempeartura ao longo de cada iteração no TemperatureSchedule (geometrica)

    protected override void Begin()
    {
        CreateFileSA(fileName);
        // Initialization
        this.newSolution = GenerateRandomSolution(targets.Count);
        CurrentSolutionCost = Evaluate(newSolution);
        base.CurrentSolution = new List<int>(newSolution);
        // Temperature = TempInicial;
        //DO NOT CHANGE THE LINES BELLOW
        AddInfoToFile(fileName, base.CurrentNumberOfIterations, CurrentSolutionCost, CurrentSolution, Temperature);
        base.CurrentNumberOfIterations++;
    }

    protected override void Step()
    {
        while (base.CurrentNumberOfIterations < base.MaxNumberOfIterations && Temperature > 0.0f)
        {
            this.newSolution = GenerateNeighbourSolution(base.CurrentSolution);
            int newSolutionCost = Evaluate(newSolution);
            double probability = Math.Exp((CurrentSolutionCost - newSolutionCost) / Temperature);
            Random random = new Random();
            if (newSolutionCost <= CurrentSolutionCost || probability > random.NextDouble())
            {
                base.CurrentSolution = newSolution;
                CurrentSolutionCost = newSolutionCost;
            }
            // Temperature = TemperatuteScheduleLinear(base.CurrentNumberOfIterations);
            Temperature = TemperatureScheduleGeometrica(Temperature);


            //DO NOT CHANGE THE LINES BELLOW
            AddInfoToFile(fileName, base.CurrentNumberOfIterations, CurrentSolutionCost, CurrentSolution, Temperature);
            base.CurrentNumberOfIterations++;
        }
    }

    public float TemperatuteScheduleLinear(int iteracao)
    {
        float n = 2.0f;
        return TempInicial - n * iteracao;
    }

    public float TemperatureScheduleGeometrica(float TemperaturaAtual)
    {
        float TemperaturaDiminui = TemperaturaAtual * (Diminuicao / 100.0f);
        TemperaturaAtual = TemperaturaAtual - TemperaturaDiminui;
        if (TemperaturaAtual <= zero)
        {
            TemperaturaAtual = 0.0f;
        }
        return TemperaturaAtual;
    }

}

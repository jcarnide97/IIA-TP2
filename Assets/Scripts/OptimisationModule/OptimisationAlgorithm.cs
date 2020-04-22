using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using System.IO;

public abstract class OptimisationAlgorithm : MonoBehaviour {


    public int MaxNumberOfIterations = 100;
    public int CurrentNumberOfIterations = 0;
    public int iterationsPerFrame = 100;
    public int randomSeed = 2020;

    protected Node startNode = null;
    protected int numberOfSteps = 0;
    protected bool TargetSequenceDefined = false;
    protected bool running = false;
    protected List<GameObject> bestSequenceFound;
    protected List<int> CurrentSolution = null;
    protected List<GameObject> targets;
    protected Dictionary<string, Dictionary<string, int>> distanceMatrix;

    protected string information = "";
    


    public void StartRunning(Node startNode) {
        TargetSequenceDefined = false;
        this.startNode = startNode;
        running = true;
        numberOfSteps = 0;
        targets = GetAllTargets();
        information = "";
        Random.InitState(randomSeed);
        Debug.Log("starting with random-seed:" + randomSeed);
        Begin();
        
    }

    // Update is called once per frame
    void Update() {
        if (running && !TargetSequenceDefined) {
            for (int i = 0; i < iterationsPerFrame; i++) {
                // this is the While CurrentNumberOfIterations < MaxNumberOfIterations
                if (CurrentNumberOfIterations < MaxNumberOfIterations) {
                    Step();
                    numberOfSteps++;
                } else {
                    // number of iterations have ended, saving the solution
                    bestSequenceFound = CreateSequenceFromSolution(CurrentSolution);
                    TargetSequenceDefined = true;
                    break;
                }
            }
        }
    }


    public bool Finished() {
        return TargetSequenceDefined == true;
    }

    public bool GetRunning() {
        return running;
    }

    public void setRunning(bool state) {
        running = state;
    }

    public int GetCost()
    {
        if(Finished())
            return Evaluate(CurrentSolution);
        return 0;
    }

    public List<int> GetCurrentSolution()
    {
        return CurrentSolution;
    }

    public List<GameObject> GetBestSequenceFound()
    {
        return CreateSequenceFromSolution(CurrentSolution);
    }

    protected List<GameObject> GetAllTargets()
    {
        return new List<GameObject>(GameObject.FindGameObjectsWithTag("Pickup"));
    }


    public int Evaluate(List<int> solution)
    {
        List<GameObject> tempTargets = CreateSequenceFromSolution(solution);
        return Evaluate(tempTargets);
    }



    public int Evaluate(List<GameObject> targets)
    {
        int totalDistance = 0;
        Node first = null;
        Node second = null;

        for (int i = 0; i < targets.Count - 1; i++)
        {
            first = GridMap.instance.NodeFromWorldPoint(targets[i].transform.position);
            second = GridMap.instance.NodeFromWorldPoint(targets[i + 1].transform.position);
            totalDistance += Evaluate(first, second);
        }

        first = GridMap.instance.NodeFromWorldPoint(targets[0].transform.position);

        totalDistance += Evaluate(startNode, first); //from agent position to the first resource in the sequence.

        return totalDistance;

    }


    protected int Evaluate(Node nodeA, Node nodeB)
    {
        return distanceMatrix[nodeA.ToString()][nodeB.ToString()];
    }

    public void setDistanceMatrix(Dictionary<string, Dictionary<string, int>> dMatrix) 
    {
        this.distanceMatrix = dMatrix;
    }


    public List<GameObject> CreateSequenceFromSolution(List<int> solution) 
    {
        List<GameObject> sequenceOfObjects = new List<GameObject>();
        foreach(int i in solution) 
        {
            sequenceOfObjects.Add(targets[i]);
        }
        return sequenceOfObjects;
    }



    public void CreateFile(string name)
    {
        File.WriteAllText(name, "Iteration,Cost,Sequence\n");
    }

    public void CreateFileSA(string name)
    {
        File.WriteAllText(name, "Iteration,Cost,Temperature,Sequence\n");
    }

    public void AddInfoToFile(string fileName, int iteration, int quality, List<int> solution)
    {
        string temp = "";
        for(int i=0; i < solution.Count - 1; i++)
        {
            temp += (solution[i] + 1) + " ";
        }
        temp += solution[solution.Count - 1] + 1;
        string content = iteration + "," + quality + "," + temp +"\n";
        Debug.Log(content);
        File.AppendAllText(fileName, content);
    }

    public void AddInfoToFile(string fileName, int iteration, int cost, List<int> solution, float temperature)
    {
        string temp = "";
        for (int i = 0; i < solution.Count - 1; i++)
        {
            temp += (solution[i] + 1) + " ";
        }
        temp += solution[solution.Count - 1] + 1;
        string content = iteration + "," + cost + "," + temperature + "," + temp + "\n";
        Debug.Log(content);
        File.AppendAllText(fileName, content);
    }


    public List<int> GenerateNeighbourSolution(List<int> solution)
    {
        List<int> neighbour = new List<int>(solution);

        int firstIndex = Random.Range(0, neighbour.Count);
        int secondIndex = Random.Range(0, neighbour.Count);
        while (firstIndex == secondIndex)
        {
            secondIndex = Random.Range(0, neighbour.Count);
        }
        int temp = neighbour[firstIndex];
        neighbour[firstIndex] = neighbour[secondIndex];
        neighbour[secondIndex] = temp;

        return neighbour;
    }



    public List<int> GenerateRandomSolution(int size)
    {
        List<int> list = new List<int>();
        List<int> shuffledList = null;
        for (int i = 0; i < size; i++)
        {
            list.Add(i);
        }
        //shuffle
        shuffledList = list.OrderBy(x => Random.value).ToList();
        return shuffledList;
    }



    // These methods should be overriden on each specific search algorithm.
    protected abstract void Begin ();
	protected abstract void Step ();


}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using SteamDataMining;

public class Map
{
    private double[,][] outputs;
    private int length;
    private int dimensions;
    private int iterations;
    private double timeConstant;
    private Random rnd = new Random();


    private List<string> labels = new List<string>();
    public List<double[]> patterns = new List<double[]>();

    private double[,,,] distanceCache;

    public Map(int dimensions, int length, int iterations, DataItem[] data)
    {
        this.length = length;
        this.dimensions = dimensions;
        this.iterations = iterations;
        this.timeConstant = iterations / Math.Log(length);
        //this.distanceCache = new double[length,length,length,length];

        // Cache distances between nodes
        /*for (int x1 = 0; x1 < length; x1++)
            for (int y1 = 0; y1 < length; y1++)
                for (int x2 = 0; x2 < length; x2++)
                    for (int y2 = 0; y2 < length; y2++)
                        distanceCache[x1,y1,x2,y2] = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));*/

        Console.WriteLine("Initialising...");
        Initialise();
        Console.WriteLine("Loading...");
        LoadData(data);
        Console.WriteLine("Normalizing...");
        NormalisePatterns();
        Console.WriteLine("Training...");
        Train();
    }

    private void Initialise()
    {
        outputs = new double[length, length][];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                outputs[i, j] = new double[dimensions];
                for (int k = 0; k < dimensions; k++)
                    outputs[i, j][k] = rnd.NextDouble();
            }
        }
    }

    private void LoadData(DataItem[] data)
    {
        foreach (var item in data)
        {
            labels.Add(item.name);
            patterns.Add(item.vectorize(dimensions));
        }
    }

    private void NormalisePatterns()
    {
        for (int j = 0; j < dimensions; j++)
        {
            double sum = 0;
            for (int i = 0; i < patterns.Count; i++)
            {
                sum += patterns[i][j];
            }
            //double average = sum / patterns.Count;
            for (int i = 0; i < patterns.Count; i++)
            {
                patterns[i][j] = patterns[i][j] / (sum == 0 ? 1 : sum);
            }
        }
    }

    private void Train()
    {
        double maxError = 0.000001;
        double currentError;
        int iteration = 0;
        var trainingSet = patterns.OrderBy(_ => rnd.Next());

        do
        {
            currentError = 0;

            trainingSet = trainingSet.OrderBy(_ => rnd.Next());
            double learningRate = LearningRate(iteration);
            double radius = Radius(iteration);

            foreach (var item in trainingSet)
                currentError += TrainPattern(item, iteration, learningRate, radius);

            iteration++;
            Console.WriteLine(currentError.ToString("0.0000000"));
        } while (currentError > maxError && iteration < iterations);
    }

    private double TrainPattern(double[] pattern, int it, double learningRate, double radius)
    {
        double error = 0;
        Tuple<int,int> winner = Winner(pattern);
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                double dist = NodeDistance(winner.Item1,winner.Item2,i,j);
                if(dist < radius)
                    error += UpdateWeights(pattern, i, j, dist, it, learningRate, radius);
            }
        }
        return Math.Abs(error / (length * length));
    }

    public double[,][] ResultMap()
    {
        return outputs;
    }

    public Tuple<int,int> Winner(double[] pattern)
    {
        Tuple<int,int> winner = null;
        double min = double.MaxValue;
        for (int i = 0; i < length; i++){
        for (int j = 0; j < length; j++)
        {
            double d = Distance(pattern, outputs[i, j]);

            if (d < min)
            {
                min = d;
                winner = new Tuple<int, int>(i, j);
            }
        }
        }
        return winner;
    }

    public double UpdateWeights(double[] pattern, int x, int y, double dist, int it, double learningRate, double radius)
    {
        double sum = 0;
        for (int i = 0; i < dimensions; i++)
        {
            double delta = learningRate * Gauss(dist, radius) * (pattern[i] - outputs[x, y][i]);
            outputs[x, y][i] += delta;
            sum += delta;
        }
        return sum / dimensions;
    }

    private double Distance(double[] vector1, double[] vector2)
    {
        double value = 0;
        for (int i = 0; i < vector1.Length; i++)
            value += (vector1[i] - vector2[i]) * (vector1[i] - vector2[i]);
        return Math.Sqrt(value);
    }

    private double NodeDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }

    private double Gauss(double dist, double radius)
    {
        return Math.Exp(-(dist * dist) / (radius * radius));
    }

    private double Radius(int it)
    {
        return Math.Exp(-it / timeConstant) * length;
    }


    private double LearningRate(int it)
    {
        return Math.Exp(-it / (double)iterations) * 0.1; // 0.1 is the starting learning rate
    }
}

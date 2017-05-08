using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SteamDataMining;

public class Map
{
    private Neuron[,] outputs; // Collection of weights.
    private int iteration; // Current iteration.
    private int length; // Side length of output grid.
    private int dimensions; // Number of input dimensions.
    private Random rnd = new Random();

    private List<string> labels = new List<string>();
    private List<double[]> patterns = new List<double[]>();

    public Map(int dimensions, int length, DataItem[] data)
    {
        this.length = length;
        this.dimensions = dimensions;
        Console.WriteLine("Initialising...");
        Initialise();
        Console.WriteLine("Loading...");
        LoadData(data);
        Console.WriteLine("Normalizing...");
        NormalisePatterns();
        Console.WriteLine("Training...");
        Train(0.0000001);
    }

    private void Initialise()
    {
        outputs = new Neuron[length, length];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                outputs[i, j] = new Neuron(i, j, length) {Weights = new double[dimensions]};
                for (int k = 0; k < dimensions; k++)
                {
                    outputs[i, j].Weights[k] = rnd.NextDouble();
                }
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
            double average = sum / patterns.Count;
            for (int i = 0; i < patterns.Count; i++)
            {
                patterns[i][j] = patterns[i][j] / average;
            }
        }
    }

    private void Train(double maxError)
    {
        double currentError;
        var trainingSet = patterns.OrderBy(_ => rnd.Next());
        do
        {
            currentError = 0;

            trainingSet = trainingSet.OrderBy(_ => rnd.Next());

            foreach (var item in trainingSet)
                currentError += TrainPattern(item);

            Console.WriteLine(currentError.ToString("0.0000000"));
        } while (currentError > maxError);
    }

    private double TrainPattern(double[] pattern)
    {
        double error = 0;
        Neuron winner = Winner(pattern);
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                error += outputs[i, j].UpdateWeights(pattern, winner, iteration);
            }
        }
        iteration++;
        return Math.Abs(error / (length * length));
    }

    public void DumpCoordinates()
    {
        for (int i = 0; i < patterns.Count; i++)
        {
            Neuron n = Winner(patterns[i]);
            Console.WriteLine("{0},{1},{2}", labels[i], n.X, n.Y);
        }
    }

    private Neuron Winner(double[] pattern)
    {
        Neuron winner = null;
        double min = double.MaxValue;
        for (int i = 0; i < length; i++)
        for (int j = 0; j < length; j++)
        {
            double d = Distance(pattern, outputs[i, j].Weights);
            if (d < min)
            {
                min = d;
                winner = outputs[i, j];
            }
        }
        return winner;
    }

    private double Distance(double[] vector1, double[] vector2)
    {
        double value = 0;
        for (int i = 0; i < vector1.Length; i++)
            value += (vector1[i] - vector2[i]) * (vector1[i] - vector2[i]);
        return Math.Sqrt(value);
    }
}

public class Neuron
{
    public double[] Weights;
    public int X;
    public int Y;
    private int length;
    private double nf;

    public Neuron(int x, int y, int length)
    {
        X = x;
        Y = y;
        this.length = length;
        nf = 1000 / Math.Log(length);
    }

    private double Gauss(Neuron win, int it)
    {
        double distance = Math.Sqrt((win.X - X) * (win.X - X) + (win.Y - Y) * (win.Y - Y));
        return Math.Exp(-(distance * distance) / (Strength(it) * Strength(it)));
    }

    private double LearningRate(int it)
    {
        return Math.Exp(-it / 1000d) * 0.1;
    }

    private double Strength(int it)
    {
        return Math.Exp(-it / nf) * length;
    }

    public double UpdateWeights(double[] pattern, Neuron winner, int it)
    {
        double sum = 0;
        for (int i = 0; i < Weights.Length; i++)
        {
            double delta = LearningRate(it) * Gauss(winner, it) * (pattern[i] - Weights[i]);
            Weights[i] += delta;
            sum += delta;
        }
        return sum / Weights.Length;
    }
}
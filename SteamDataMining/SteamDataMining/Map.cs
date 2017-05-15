using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SteamDataMining;

public class Map
{
    private double[,][] outputs; // Collection of weights.
    private int iteration; // Current iteration.
    private int length; // Side length of output grid.
    private int dimensions; // Number of input dimensions.
    private Random rnd = new Random();

    private double nf;

    private List<string> labels = new List<string>();
    private List<double[]> patterns = new List<double[]>();

    public Map(int dimensions, int length, DataItem[] data)
    {
        this.length = length;
        this.dimensions = dimensions;
        nf = 1000 / Math.Log(length);

        Console.WriteLine("Initialising...");
        Initialise();
        Console.WriteLine("Loading...");
        LoadData(data);
        Console.WriteLine("Normalizing...");
        NormalisePatterns();
        Console.WriteLine("Training...");
        Train(0.001);
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
            double average = sum / patterns.Count;
            for (int i = 0; i < patterns.Count; i++)
            {
                patterns[i][j] = patterns[i][j] / (average == 0 ? 1 : average);
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
        Tuple<int,int> winner = Winner(pattern);
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                error += UpdateWeights(pattern, winner.Item1, winner.Item2, i, j, iteration);
            }
        }
        iteration++;
        return Math.Abs(error / (length * length));
    }

    public void DumpCoordinates()
    {
        for (int i = 0; i < patterns.Count; i++)
        {
            Tuple<int, int> n = Winner(patterns[i]);
            Console.WriteLine("{0},{1},{2}", labels[i], n.Item1, n.Item2);
        }
    }

    public double[,][] ResultMap()
    {
        return outputs;
    }

    private Tuple<int,int> Winner(double[] pattern)
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

    public double UpdateWeights(double[] pattern, int winX, int winY, int nodeX, int nodeY, int it)
    {
        double sum = 0;
        for (int i = 0; i < dimensions; i++)
        {
            double delta = LearningRate(it) * Gauss(winX, winY, nodeX, nodeY, it) * (pattern[i] - outputs[nodeX, nodeY][i]);
            outputs[nodeX, nodeY][i] += delta;
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


    private double Gauss(int winX, int winY, int nodeX, int nodeY, int it)
    {
        double distance = Math.Sqrt((winX - nodeX) * (winX - nodeX) + (winY - nodeY) * (winY - nodeY));
        return Math.Exp(-(distance * distance) / (Strength(it) * Strength(it)));
    }

    private double Strength(int it)
    {
        return Math.Exp(-it / nf) * length;
    }


    private double LearningRate(int it)
    {
        return Math.Exp(-it / 1000d) * 0.1;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

class MultiArmedBandit
{
    static void Main(string[] args)
    {
        // Initialize probability distributions for 4 restaurants (A, B, C, D)
        Dictionary<string, double[]> probabilities = new Dictionary<string, double[]>()
        {
            {"A", new double[] {0.2, 0.2, 0.2, 0.2, 0.2}},
            {"B", new double[] {0.2, 0.2, 0.2, 0.2, 0.2}},
            {"C", new double[] {0.2, 0.2, 0.2, 0.2, 0.2}},
            {"D", new double[] {0.2, 0.2, 0.2, 0.2, 0.2}},
        };

        // Simulate iterations
        for (int i = 1; i <= 4; i++)
        {
            Console.WriteLine($"Iteration {i}: Probability Distributions");
            PrintProbabilities(probabilities);

            // Sample from each restaurant
            var sampledValues = new Dictionary<string, int>();
            foreach (var restaurant in probabilities.Keys)
            {
                sampledValues[restaurant] = Sample(probabilities[restaurant]);
            }

            // Select the restaurant with the highest sampled value
            var selectedRestaurant = sampledValues.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            var selectedValue = sampledValues[selectedRestaurant];

            // Simulate feedback (a value from 0 to 5)
            var feedback = GetFeedback();

            Console.WriteLine($"Selected Restaurant: {selectedRestaurant} with sample {selectedValue}, Feedback: {feedback}");

            // Update the probabilities for the selected restaurant based on feedback
            UpdateProbabilities(probabilities, selectedRestaurant, feedback);
        }
    }

    // Function to sample from the probability distribution of a restaurant
    static int Sample(double[] probabilities)
    {
        Random rand = new Random();
        double r = rand.NextDouble();
        double cumulative = 0.0;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (r < cumulative)
            {
                return i + 1; // Return the rating (1 to 5)
            }
        }
        return probabilities.Length; // Fallback
    }

    // Function to update the probability distribution based on feedback
    static void UpdateProbabilities(Dictionary<string, double[]> probabilities, string restaurant, int feedback)
    {
        double[] prob = probabilities[restaurant];

        // Increase the probability of the feedback rating
        prob[feedback] += 0.05;

        // Also slightly increase the surrounding probabilities if feedback is not at the extremes (0 or 5)
        if (feedback > 0) prob[feedback - 1] += 0.03; // Slightly increase lower bound
        if (feedback < 4) prob[feedback + 1] += 0.03; // Slightly increase upper bound

        // Normalize the probabilities so that they sum to 1
        double sum = prob.Sum();
        for (int i = 0; i < prob.Length; i++)
        {
            prob[i] /= sum;
        }
    }

    // Function to simulate feedback (a random value from 0 to 5)
    static int GetFeedback()
    {
        Random rand = new Random();
        return rand.Next(0, 5); // Return a random value between 0 and 5
    }

    // Function to print the probability distribution for each restaurant
    static void PrintProbabilities(Dictionary<string, double[]> probabilities)
    {
        foreach (var entry in probabilities)
        {
            Console.WriteLine($"{entry.Key}: {string.Join(", ", entry.Value.Select(p => p.ToString("F2")))}");
        }
        Console.WriteLine();
    }
}

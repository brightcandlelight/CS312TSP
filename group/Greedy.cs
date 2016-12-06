using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace group
{
    class Greedy
    {
        internal void solve(out ArrayList route, City[] cities, out string time)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            bool[] visited;
            double[] solutions = new double[cities.Length];
            ArrayList[] routes = new ArrayList[cities.Length];
            for (int i = 0; i < cities.Length; i++)
            {
                visited = new bool[cities.Length];
                visited[i] = true;
                int currentCity = i;
                double totalDist = 0;
                routes[i] = new ArrayList();
                routes[i].Add(cities[currentCity]);
                while (routes[i].Count < cities.Length)
                {
                    double closestDist;
                    currentCity = ClosestUnvisitedCity(out closestDist, cities, visited, currentCity);
                    routes[i].Add(cities[currentCity]);
                    visited[currentCity] = true;
                    totalDist += closestDist;
                }
                solutions[i] = totalDist;
            }
            double shortestRoute = double.MaxValue;
            int bestStartNode = 0;
            for (int i = 0; i < cities.Length; i++)
            {
                if (solutions[i] < shortestRoute)
                {
                    shortestRoute = solutions[i];
                    bestStartNode = i;
                }
            }
            timer.Stop();
            time = timer.Elapsed.ToString();
            route = routes[bestStartNode];
        }

        private int ClosestUnvisitedCity(out double closestDist, City[] cities, bool[] visited, int currentCity)
        {
            closestDist = double.MaxValue;
            int closestCity = currentCity;
            for (int i = 0; i < cities.Length; i++)
            {
                if (!visited[i])
                {
                    double distance = cities[currentCity].costToGetTo(cities[i]);
                    if (distance < closestDist)
                    {
                        closestDist = distance;
                        closestCity = i;
                    }
                }
            }
            return closestCity;
        }
    }
}
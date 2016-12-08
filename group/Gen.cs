using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace group
{
    class Population
    {
        //all the individuals that will be evolving
        List<ArrayList> routes;
        public int PopulationSize()
        {
            return routes.Count;
        }

        //if cities is null, O(n), otherwise O(n^3)
        public Population(int populationSize, City[] cities = null)
        {
            if (cities != null)
            {
                Greedy g = new Greedy();
                //O(n^3)
                List<ArrayList> greedyRoutes = g.getRoutes(cities);
                //shuffle routes
                //O(n)
                int n = greedyRoutes.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    ArrayList value = greedyRoutes[k];
                    greedyRoutes[k] = greedyRoutes[n];
                    greedyRoutes[n] = value;
                }
                //worst O(m*n) where m is population size and n is number of cities
                if (populationSize > greedyRoutes.Count)
                {
                    routes = new List<ArrayList>();
                    for (int i = 0; i < greedyRoutes.Count; i++)
                    {
                        routes.Add(new ArrayList());
                        routes[i] = greedyRoutes[i];
                    }
                    for (int i = greedyRoutes.Count; i < populationSize; i++)
                    {
                        routes.Add(new ArrayList());
                        //O(n)
                        routes[i] = Spawn(cities);
                    }

                }
                //O(k)
                else { 
                
                    routes = greedyRoutes.GetRange(0, populationSize);
                }
            }
            //O(n) where n is population size
            else
            {
                routes = new List<ArrayList>();
                for (int i = 0; i < populationSize; i++)
                {
                    routes.Add(new ArrayList());

                    //this is to see if the constructor should spawn or not - this was for previous version that did not use the greedy results as population. Now unreachable
                    if (cities != null)
                    {
                        routes[i] = Spawn(cities);
                    }
                }
            }
        }
        private static Random rng = new Random();
        //O(2n) -> O(n)
        private ArrayList Spawn(City[] cities)
        {
            ArrayList route = new ArrayList();
            for (int i = 0; i < cities.Length; i++)
            {
                route.Add(cities[i]);
            }
            //Randomly shuffling the routes
            int n = route.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                object value = route[k];
                route[k] = route[n];
                route[n] = value;
            }
            return route;
        }

        public void setRoute(int i, ArrayList r)
        {
            routes[i] = r;
        }

        public ArrayList getRoute(int i)
        {
            return routes[i];
        }

        //Fitness function - gets fittest individual - aka Route that costs least
        //O(m* 2n) -> O(m*n) where m is populationSize and n is number of cities
        public ArrayList SelectFittest()
        {
            ArrayList fittest = routes[0];
            for (int i = 1; i < routes.Count; i++)
            {
                if (GetCost(fittest) > GetCost(getRoute(i)))
                {
                    fittest = getRoute(i);
                }
            }
            return fittest;
        }


        //This method is copied from the main project
        //O(n) where n is number of cities
        private double GetCost(ArrayList Route)
        {
            // go through each edge in the route and add up the cost. 
            int x;
            City here;
            double cost = 0D;

            for (x = 0; x < Route.Count - 1; x++)
            {
                here = Route[x] as City;
                cost += here.costToGetTo(Route[x + 1] as City);
            }

            // go from the last city to the first. 
            here = Route[Route.Count - 1] as City;
            cost += here.costToGetTo(Route[0] as City);
            return cost;
        }

        //O(n)
        internal Population Remove(ArrayList route)
        {
            routes.Remove(route);
            return this;
        }
    }
    class Gen
    {
        private static Random rng = new Random();

        //O(n^2 * m) where m is populationSize and n is num cities
        //Space: O(n*m) where m is populationSize and n is num cities
        public Population Evolve(Population p)
        {
            //O(n) where n is population size
            Population evolved = new Population(p.PopulationSize());

            //O(m * n) where m is population size and n is cities size
            evolved.setRoute(0, p.SelectFittest());

            //O(n^2 * m) where m is populationSize and n is num cities
            for (int i = 1; i < evolved.PopulationSize(); i++)
            {
                //O(n*m) where n is num of cities and m is parentPoolSize
                Tuple<ArrayList, ArrayList> t = SelectParents(p);
                //O(n^2)
                ArrayList offspring = Crossover(t.Item1, t.Item2);
                evolved.setRoute(i, offspring);
            }

            //O(n*m) where m is populationSize and n is num cities
            for (int i = 1; i < evolved.PopulationSize(); i++)
            {
                //O(n)
                Mutate(evolved.getRoute(i));
            }

            return evolved;
        }

        //Swapping mutation
        //O(n) n is num of cities
        private void Mutate(ArrayList route)
        {
            for (int a = 0; a < route.Count; a++)
            {
                if (rng.NextDouble() < mutationRate) {
                    int b = rng.Next(0, route.Count);

                    object city1 = route[a];
                    object city2 = route[b];

                    route[b] = city1;
                    route[a] = city2;
                }
            }
        }

        //O(n^2) n is num of cities
        private ArrayList Crossover(ArrayList parent1, ArrayList parent2)
        {
            //O(n) n num of cities
            ArrayList offspring = new ArrayList();
            for (int i = 0; i < parent1.Count; i++)
            {
                offspring.Add(null);
            }
           
            int start = rng.Next(0, parent1.Count);
            int end = rng.Next(0, parent1.Count);
            if (start > end)
            {
                int temp = start;
                start = end;
                end = temp;
            }
            //worstCase O(n) n num of cities
            for (int i = start; i <= end; i++)
            {
                offspring[i] = parent1[i];
            }
            //worst case O(2n^2) -> O(n^2)
            for (int i = 0; i < parent2.Count; i++)
            {
                //faster method than contains?
                //O(n)
                if (!offspring.Contains(parent2[i]))
                {
                    //O(n)
                    for (int k = 0; k < offspring.Count; k++)
                    {
                        if (offspring[k] == null)
                        {
                            offspring[k] = parent2[i];
                            break;
                        }
                    }
                }
            }
            return offspring;
        }

        //O(n*m) where n is num of cities and m is parentPoolSize
        private Tuple<ArrayList, ArrayList> SelectParents(Population p)
        {
            //O(n) where n is parentPoolSize
            Population parentPool = new Population(parentPoolSize);

            //O(n) where n is parentPoolSize
            for (int i = 0; i < parentPoolSize; i++)
            {
                int ranIndex = rng.Next(0, p.PopulationSize());
                parentPool.setRoute(i, p.getRoute(ranIndex));
            }
            //O(n*m) where n is num of cities and m is parentPoolSize
            ArrayList parent1 = parentPool.SelectFittest();
            //O(n) where n is population size
            p = p.Remove(parent1);
            //O(n) where n is parentPoolSize
            for (int i = 0; i < parentPoolSize; i++)
            {
                int ranIndex = rng.Next(0, p.PopulationSize());
                parentPool.setRoute(i, p.getRoute(ranIndex));
            }
            //O(n*m) where n is num of cities and m is parentPoolSize
            ArrayList parent2 = parentPool.SelectFittest();

            return new Tuple<ArrayList, ArrayList>(parent1, parent2);
        }

        double mutationRate;
        int parentPoolSize;

        
        //Time for smaller amounts of cities: O(n^2 * m) where m is populationSize and n is num cities
        //Time for larger: O(n^3) thanks to initial greedy population
        //Space for smaller: O(n*m)
        //Space for larger: O(n^2) thanks to initial greedy population
        public void solve(City[] cities, out ArrayList route, out string time, int populationSize, int evolutionRounds, double mutationRate, int parentPoolSize, int maxTime)
        {
            this.mutationRate = mutationRate;
            this.parentPoolSize = parentPoolSize;
            Stopwatch timer = new Stopwatch();

            timer.Start();
            //O(n^3) with greedy initial population
            Population p = new Population(populationSize, cities);
            //O(n^2 * m) where m is populationSize and n is num cities
            //O(n^2 * m * l) where m is populationSize and n is num cities and l is evolutionRounds
            for (int i = 1; i<= evolutionRounds; i++)
            {
                if (timer.ElapsedMilliseconds > maxTime) {
                    break;
                }
                //O(n^2 * m) where m is populationSize and n is num cities
                p = Evolve(p);
            }
            //O(m * n)
            route = p.SelectFittest();
            timer.Stop();
            time = timer.Elapsed.ToString();
        }
    }
}

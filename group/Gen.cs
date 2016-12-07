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

        public Population(int populationSize, City[] cities = null)
        {
            if (cities != null)
            {
                Greedy g = new Greedy();
                List<ArrayList> greedyRoutes = g.getRoutes(cities);
                //shuffle routes
                int n = greedyRoutes.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    ArrayList value = greedyRoutes[k];
                    greedyRoutes[k] = greedyRoutes[n];
                    greedyRoutes[n] = value;
                }
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
                        routes[i] = Spawn(cities);
                    }

                } else { 
                
                    routes = greedyRoutes.GetRange(0, populationSize);
                }
            }
            else
            {
                routes = new List<ArrayList>();
                for (int i = 0; i < populationSize; i++)
                {
                    routes.Add(new ArrayList());

                    //this is to see if the constructor should spawn or not
                    if (cities != null)
                    {
                        routes[i] = Spawn(cities);
                    }
                }
            }
        }
        private static Random rng = new Random();
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

        internal Population Remove(ArrayList route)
        {
            routes.Remove(route);
            return this;
        }
    }
    class Gen
    {
        private static Random rng = new Random();
        
        public Population Evolve(Population p)
        {
            Population evolved = new Population(p.PopulationSize());

            evolved.setRoute(0, p.SelectFittest());

            for (int i = 1; i < evolved.PopulationSize(); i++)
            {
                Tuple<ArrayList, ArrayList> t = SelectParents(p);
                ArrayList offspring = Crossover(t.Item1, t.Item2);
                evolved.setRoute(i, offspring);
            }

            for (int i = 1; i < evolved.PopulationSize(); i++)
            {
                Mutate(evolved.getRoute(i));
            }

            return evolved;
        }

        //Swapping mutation
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

        private ArrayList Crossover(ArrayList parent1, ArrayList parent2)
        {
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
            for (int i = start; i <= end; i++)
            {
                offspring[i] = parent1[i];
            }

            for (int i = 0; i < parent2.Count; i++)
            {
                //faster method than contains?
                if (!offspring.Contains(parent2[i]))
                {
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

        private Tuple<ArrayList, ArrayList> SelectParents(Population p)
        {
            Population parentPool = new Population(parentPoolSize);

            for (int i = 0; i < parentPoolSize; i++)
            {
                int ranIndex = rng.Next(0, p.PopulationSize());
                parentPool.setRoute(i, p.getRoute(ranIndex));
            }

            ArrayList parent1 = parentPool.SelectFittest();
            p = p.Remove(parent1);
            for (int i = 0; i < parentPoolSize; i++)
            {
                int ranIndex = rng.Next(0, p.PopulationSize());
                parentPool.setRoute(i, p.getRoute(ranIndex));
            }

            ArrayList parent2 = parentPool.SelectFittest();

            return new Tuple<ArrayList, ArrayList>(parent1, parent2);
        }

        double mutationRate;
        int parentPoolSize;

        public void solve(City[] cities, out ArrayList route, out string time, int populationSize, int evolutionRounds, double mutationRate, int parentPoolSize, int maxTime)
        {
            this.mutationRate = mutationRate;
            this.parentPoolSize = parentPoolSize;
            Stopwatch timer = new Stopwatch();

            timer.Start();
            Population p = new Population(populationSize, cities);
            for (int i = 1; i<= evolutionRounds; i++)
            {
                if (timer.ElapsedMilliseconds > maxTime) {
                    break;
                }
                p = Evolve(p);
            }
            route = p.SelectFittest();
            timer.Stop();
            time = timer.Elapsed.ToString();
        }
    }
}

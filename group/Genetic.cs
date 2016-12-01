using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace group {
    class individual {
        public int[] path;
        public double cost;

        public individual() { }

        public individual(int l) {
            path = new int[l];
        }

        public individual(individual old) {
            this.path = new int[old.path.Length];
            for (int x =0;x<old.path.Length;x++) {
                this.path[x] = old.path[x];
            }
            this.cost= old.cost;
        }

        public void copy(int[] old) {
            //this.path = new int[old.Length];
            for (int x = 0; x < path.Length; x++) {
                this.path[x] = old[x];
            }
        }
    }

    class Genetic {
        static readonly int SEED = 677;
        static readonly int POPULATION = 1000;
        static readonly int PERCENT_CHANCE_MUTATION = 10;
        public static readonly double INFINITY = Double.PositiveInfinity;

        Random r;
        List<individual> species = new List<individual>(POPULATION);
        group.BnB.node cityBase;
        ProblemAndSolver.TSPSolution bssf;
        ProblemAndSolver pAS = new ProblemAndSolver();

        int dec(int num, int l) {
            num--;
            if (num < 0) {
                return l - 1;
            }
            return num;
        }

        int inc(int num, int l) {
            num++;
            if (num > l-1) {
                return 0;
            }
            return num;
        }
        
        bool isValidConnection(int city1, int city2) {
            // Make sure it is not infinity
            if (cityBase.costs[city1][city2] < INFINITY) {
                return true;
            }
            return false;
        }

        // A little overkill but this will work
        int[] mutate(int[] path, int l) {
            int rand = r.Next(100);
            if (rand < PERCENT_CHANCE_MUTATION) {
                // Pick a place and find a city that would fit there instead that preserves the path
                // and that swapping them would preserve the path on the other side too
                rand = r.Next(path.Length);
                int newCity = r.Next(path.Length);
                bool prevConnect1 = isValidConnection(path[dec(rand,l)],newCity);
                bool nextConnect1 = isValidConnection(newCity,path[inc(rand,l)]);
                bool prevConnect2 = isValidConnection(path[dec(newCity,l)], rand);
                bool nextConnect2 = isValidConnection(rand,path[inc(newCity,l)]);
                while (!prevConnect1 || !nextConnect1 || !prevConnect2 || !nextConnect2) {
                    newCity = r.Next(path.Length);
                    prevConnect1 = isValidConnection(path[dec(rand, l)], newCity);
                    nextConnect1 = isValidConnection(newCity, path[inc(rand, l)]);
                    prevConnect2 = isValidConnection(path[dec(newCity, l)], rand);
                    nextConnect2 = isValidConnection(rand, path[inc(newCity, l)]);
                }

                // Swap with the other one
                int temp = path[rand];
                path[rand] = newCity;
                path[newCity] = temp;
            }
            return path;
        }


        int[] crossover(ref int[] path1, ref int[] path2, int l) {
            int rand = r.Next(1,path1.Length);
            int[] temp = new int[l];

            // See if it will work
            bool connect = isValidConnection(path1[dec(rand,l)], path2[rand]);
            while (!connect) {
                rand = r.Next(0, path1.Length);
                connect = isValidConnection(path1[dec(rand, l)], path2[rand]);
            }

            // Merge
            for (int x = 0; x < rand; x++) {
                temp[x] = path1[x];
            }
            for (int x = rand; x < l; x++) {
                temp[x] = path2[x];
            }

            return temp;
        }

        int[] reproduce(ref int[] path1, ref int[] path2, int l) {
            return mutate(crossover(ref path1, ref path2,l),l);
        }

        // Returns the index of the parent
        // Pass in the other parent so we don't get the same two parents
        int parentSelection(int otherParent, int l) {
            // Probabilistically based on fitness
            int decision =0;

            // TODO: UPDATE TO SOMETHING COOL
            // Right now lets just pick any two parents
            decision = r.Next(l);
            while (decision == otherParent) {
                decision = r.Next(l);
            }

            //
            return decision;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        void init(City[] Cities) {
            r = new Random(SEED);
            species = new List<individual>(POPULATION);
            // Initialize to random cities
            for (int j = 0; j < POPULATION; j++) {
                species.Add(new individual(Cities.Length));

                // Default solve problem
                int[] perm = new int[Cities.Length];
                //{
                int i, swap, temp1, count = 0;
                string[] results = new string[3];
                ArrayList Route = new ArrayList();
                Random rnd = new Random(j);
                 
                do {
                    for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                        perm[i] = i;
                    for (i = 0; i < perm.Length; i++) {
                        swap = i;
                        while (swap == i)
                            swap = rnd.Next(0, Cities.Length);
                        temp1 = perm[i];
                        perm[i] = perm[swap];
                        perm[swap] = temp1;
                    }
                    Route.Clear();
                    for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                    {
                        Route.Add(Cities[perm[i]]);
                    }
                    bssf = new ProblemAndSolver.TSPSolution(Route);
                    count++;
                } while (pAS.costOfBssf() == double.PositiveInfinity);                // until a valid route is found
                //}
                

                species[j].copy(perm);
                //int[] temp1 = ;
                //species.Add(new individual(temp1));
                species[j].cost = determineFitness(ref Cities, species[j].path);
            }

            // Borrowing my generate cost function
            BnB temp = new BnB();
            temp.generateCosts(Cities, out cityBase);
        }

        public void solve(ref City[] Cities, int maxTime, ref string timeOut, ref int bssfUpdates, ref ProblemAndSolver.TSPSolution bssf, ref double bssfCost) {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            
            // At the very least, stop when you run out of time
            while (timer.ElapsedMilliseconds < maxTime) {
                init(Cities);

                // Pick two parents
                int indexA = parentSelection(-1, Cities.Length);
                int[] parentA = species[indexA].path;

                int[] parentB = species[parentSelection(indexA, Cities.Length)].path;


                // Have them reproduce
                individual child = new individual();
                child.path = reproduce(ref parentA, ref parentB, Cities.Length);
                child.cost = determineFitness(ref Cities, child.path);

                // Store the child somewhere

                // TODO - DO SOMETHING COOL
                // for now just store at least cost guy. If none, don't keep
                for (int i = 0; i < Cities.Length; i++) {
                    if (species[i].cost > child.cost) {
                        species[i] = new individual(child);
                        bssfUpdates++;
                        break;
                    }
                }
            }

            // Find out the best
            int best=0;
            for (int i = 1; i < Cities.Length; i++) {
                if (species[i].cost < species[best].cost) {
                    best = i;
                }
            }

            ArrayList Route = new ArrayList();
            Route.Clear();
            for (int i = 0; i < species[best].path.Length; i++) {
                Route.Add(Cities[species[best].path[i]]);
            }
            bssf = new ProblemAndSolver.TSPSolution(Route);
            bssfCost = bssf.costOfRoute();

            // Return variables
            timer.Stop();
            timeOut = timer.Elapsed.ToString();
        }

        // Determine the fitness by calculating the length.
        // Space and time complexity = o(n).
        double determineFitness(ref City[] Cities, int[] path) {
            ArrayList Route = new ArrayList();
            Route.Clear();
            for (int i = 0; i < Cities.Length; i++) {
                Route.Add(Cities[path[i]]);
            }
            ProblemAndSolver.TSPSolution bssf = new ProblemAndSolver.TSPSolution(Route);
            return bssf.costOfRoute();
        }
    }
}

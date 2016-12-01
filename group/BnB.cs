using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace group {
    public class BnB {
        public static double INFINITY = Double.PositiveInfinity;
        List<node> nodes;

        public BnB() {
            nodes = new List<node>();
        }

        class node {
            public double[][] costs; // The cost array used to compute the minimum bound
            public int depth;
            public List<int> path;
            public double bound;
            public bool deleted;

            // Gets the last item in the past
            public int getLastVisitedNode() {
                return path[path.Count - 1];
            }

            public node() { }

            // deep copy
            // Space complexity = o(n^2) from passing in/making a new item with a 2d array in it. n = # of cities
            // Time complexity = o(n^2).
            public node(node copy) {
                int length = copy.costs.Length;
                costs = new double[length][];
                for (int i=0;i<length;i++){
                    costs[i] = new double[length];
                }

                for (int x=0;x<length;x++){
                    for (int y=0;y<length;y++) {
                        costs[x][y] = copy.costs[x][y];
                    }
                }

                depth = copy.depth;
                path = new List<int>();
                for (int x =0;x<copy.path.Count;x++) {
                    path.Add(copy.path[x]);
                }
                bound = copy.bound;
                deleted = false;
            }
        }

        // Make the initial temp node and populate the costs by calling costToGetTo.
        // Space complexity = o(n^2) from passing in a node with a 2d array in it. n = # of cities
        // Time complexity = o(n^2).
        void generateCosts(City[] Cities, out node temp) {
            temp = new node();
            temp.costs = new double[Cities.Length][];
            for (int i=0;i<Cities.Length;i++){
                temp.costs[i] = new double[Cities.Length];
            }

            for (int i=0;i<Cities.Length;i++){
                for (int j=0;j<Cities.Length;j++) {
                    if (i == j) {
                        temp.costs[i][j] = INFINITY;
                        continue;
                    }

                    temp.costs[i][j] = Cities[i].costToGetTo(Cities[j]);
                }
            }
        }

        // Decrements a row or column to get it to be zero.
        // If there's already a zero it skips.
        // Space complexity = o(n^2) from passing in a node with a 2d array in it. n = # of cities
        // Time complexity = o(n).
        void dec(ref node A, int i, int j) {
            double least = INFINITY;

            // i is the row #. j is the col number. 
            // If one is -1 we know to decrement by the other one
            if (j == -1) {
                // First find the least cost in that row
                for (int y = 0; y < A.costs[i].Length; y++) {
                    if (A.costs[i][y] < least) {
                        least = A.costs[i][y];
                        if (least <= 0) {
                            return;
                        }
                    }
                }

                if (least == INFINITY) { // Skip if there aren't any edges
                    return;
                }

                // Decrement all of them by that (if not INFINITY)
                for (int y = 0; y < A.costs[i].Length; y++) {
                    if (A.costs[i][y] != INFINITY) {
                        A.costs[i][y] -= least;
                    }
                }
            } else {
                // First find the least cost in that column
                for (int x = 0; x < A.costs.Length; x++) {
                    if (A.costs[x][j] < least) {
                        least = A.costs[x][j];
                        if (least <= 0) {
                            return;
                        }
                    }
                }

                if (least == INFINITY) { // Skip if there aren't any edges
                    return;
                }

                // Decrement all of them by that (if not INFINITY)
                for (int x = 0; x < A.costs.Length; x++) {
                    if (A.costs[x][j] != INFINITY) {
                        A.costs[x][j] -= least;
                    }
                }
            }
            // Add that cost to A.
            if (least < 0) {
                A.bound += least;
            }
            A.bound += least;
        }


        // Space complexity = o(n^2) from passing in a node with a 2d array in it. n = # of cities
        // Time complexity = o(n^2). dec is n and we have an outer for loop.
        void reducer(ref node A) {
            // Decrement by rows
            for (int i = 0; i < A.costs.Length; i++) {
                dec(ref A, i, -1);
            }

            // Same for each col
            for (int i = 0; i < A.costs[0].Length; i++) {
                dec(ref A, -1, i);
            }
        }

        // Space complexity = o(n^2) from passing in a node with a 2d array in it. n = # of cities
        // Time complexity = o(n^2). By calling reducer
        void walkToNextNode(ref node A, int from, int to) {
            // First add cost (if from-to is not 0)
            if (A.costs[from][to] < 0) {
                A.bound += A.costs[from][to];
            }
            A.bound += A.costs[from][to];

            // First infinity out FROM ROW and TO COL
            // (can do at the same time because the array is a square)
            for (int i = 0; i < A.costs.Length; i++) {
                A.costs[from][i] = INFINITY;
                A.costs[i][to] = INFINITY;
            }

            // Infinity out the backwards edge
            A.costs[to][from] = INFINITY;

            // reduce again
            reducer(ref A);

            // add to path
            A.path.Add(to);
            A.depth++;
        }

        // Space complexity = o(n^2) from passing in a node with a 2d array in it. n = # of cities
        // Time complexity = o(1).
        double queueWeight(node A) {
            return queueWeight(A.bound, A.depth);
        }

        // Space complexity = o(1).
        // Time complexity = o(1).
        double queueWeight(double bound, int depth) {
            // minus is important because it will pick the smallest
            int depthWeight = 90; // The weight of 90 seems to work best given my experiementations
            return bound - depth*depthWeight; 
        }

        // Adds to the node list. 
        // Space complexity = o(n^2) from passing in a node with a 2d array in it. n = # of cities
        // Time complexity = o(v). Worse case is one where we have to add one after v-1 sets of nodes have already been stored and none can be replaced.
        int addToNodeList(ref List<node> nodes, node node) {
            for (int i = 0; i < nodes.Count; i++) {
                if (nodes[i].deleted == true) {
                    nodes[i] = node;
                    return i;
                }
            }
            nodes.Add(node);
            return nodes.Count-1;
        }

        // Space complexity = o(n^2*v). For each edge (v) we need an array of nodes^2.
        // Time complexity = o(v*n*(n^2+v)) or o(v*n^3 + nv^2). For each edge (v) we need to go through a loop n times that calls walkToNextNode which is n^2. And addToNodeList which is v.
        public void solve(priorityQueue H, int startNode, City[] Cities, ref double costOfBssf, ref int countHittingBottom, ref ProblemAndSolver.TSPSolution bssf, int maxTime, ref string timeOut, ref int bssfUpdates, ref int maxUsedSize, ref int pruned, ref int generatedNodes) {
            // variables
            int cityCount;
            ArrayList Route = new ArrayList();
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // First set the first node
            generatedNodes++;
            node start;
            generateCosts(Cities, out start);
            //testGenerateCosts(Cities,out start);
            cityCount = start.costs.Length; // dynamic so it is decoupled to City[]
            int maxQueue = 10000 * cityCount;
            start.path = new List<int>(maxQueue);
            start.path.Add(startNode);
            reducer(ref start);
            addToNodeList(ref nodes, start);

            // Make the queue and send it in
            H.makequeue(maxQueue);
            H.insert(startNode, 0); // doesn't matter the weight. Just use 0

            // Note max_time is in milliseconds
            while (!H.isEmpty() && timer.ElapsedMilliseconds < maxTime) {
                int u = H.deletemin();
                node current = nodes[u]; // u is a pointer to the nodes list.

                // If worse then what we have found so far, prune.
                if (current.bound >= costOfBssf) {
                    // We will prune it. Inc pruned.
                    pruned++;
                    // clear from node list by allowing to skip to end
                } else {
                    bool didGenerateNodes = false;
                    // generate children
                    for (int i = 0; i < cityCount; i++) {
                        // skip if you can't go there.
                        if (current.costs[current.getLastVisitedNode()][i] == INFINITY) { continue; }

                        generatedNodes++; // We are making a new node
                        didGenerateNodes = true;
                        node temp = new node(current); ; // Deep copy
                        walkToNextNode(ref temp, current.getLastVisitedNode(), i);
                        if (temp.bound >= costOfBssf) { // if it's not worth it, don't add to queue
                            pruned++;
                        } else { // otherwise add to array and queue
                            int index = addToNodeList(ref nodes, temp);
                            if (!H.insert(index, queueWeight(temp))) {
                                throw new Exception("Out of space");
                            }
                        }
                    }

                    // If you haven't generated any nodes (ie you can't go anywhere), see if you are done
                    if (!didGenerateNodes) {
                        if (current.path.Count == cityCount + 1) {
                            // You have got to them all and back to start, good job!
                            // Save
                            Route.Clear();
                            for (int i = 0; i < current.path.Count; i++) {
                                Route.Add(Cities[current.path[i]]);
                            }
                            bssf = new ProblemAndSolver.TSPSolution(Route);
                            costOfBssf = bssf.costOfRoute();
                            //costOfBssf = current.bound;
                            bssfUpdates++;
                        }
                    }
                }
                // clear from node list
                nodes[u].deleted = true;
            }
            // You are done so update variables to pass back
            timer.Stop();
            timeOut = timer.Elapsed.ToString();
            maxUsedSize = H.maxUsedSize;
        }
    }
}

/*double[] test1 = {double.PositiveInfinity, 7,3,12};
double[] test2 = { 3, double.PositiveInfinity, 6, 14 };
double[] test3 = { 5, 8, double.PositiveInfinity, 6 };
double[] test4 = { 9, 3, 5, double.PositiveInfinity };*/

/*static double d = double.PositiveInfinity;
double[] test1 = { d, 1,3,d,d};
double[] test2 = { d,d,1,3,d };
double[] test3 = { 3,d,d,1,2 };
double[] test4 = { d,3,d,d,1 };
double[] test5 = { 30,d,2,d,d };

private void testGenerateCosts(City[] Cities, out node temp) {
    // This one works :)
    int size = 5;
    temp = new node();
    temp.costs = new double[size][];
    for (int i = 0; i < size; i++) {
        temp.costs[i] = new double[size];
    }

    for (int i = 0; i < size; i++) {
        temp.costs[0][i] = test1[i];
        temp.costs[1][i] = test2[i];
        temp.costs[2][i] = test3[i];
        temp.costs[3][i] = test4[i];
        temp.costs[4][i] = test5[i];
    }
}*/

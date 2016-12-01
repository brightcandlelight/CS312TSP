using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;


namespace group {
    abstract public class priorityQueue {
        // Make the queue with the size
        abstract public void makequeue(int size);

        // Get the item with the next minumum weight. Return the index of the corrisponding node
        abstract public int deletemin();

        // Decrease the key with the given float value (not used for this lab)
        abstract public void decreasekey(int index, float value);

        // Determine if the queue is empty.
        abstract public bool isEmpty();

        // Insert an item into the queue with the given pointer to a node and weight. Returns false if full
        abstract public bool insert(int pointer, double value);

        // Value of the maximum used items in the array at a given time.
        public int maxUsedSize;
    }

    // Note the array implimentation has a space complexity of o(n) where n is the size of the queue.
    public class array : priorityQueue {
        static int INFINITY = 999999999;
        int[] index;
        double[] weight;
        bool[] settled;
        int usedEver = 0; // How much has ever been used
        int usedSize = 0; // How much is currently being used

        // See if the node is empty
        // Recursive space complexity = o(1). Time complexity = o(n) worse case (when usedEver = n)
        public override bool isEmpty() {
            // Empty if not initialized
            if (weight == null || weight.Length < 0) {
                return true;
            }
            // Not empty if one of them (!=infinity) isn't settled.
            for (int i = 0; i < usedEver; i++) {
                if (!settled[i] && weight[i] != INFINITY) {
                    return false;
                }
            }
            return true;
        }

        // Just initialize the queue with array weights, pointers, and settled bool. 
        // Recursive space complexity = o(1). Time complexity = o(n).
        public override void makequeue(int size) {
            weight = new double[size];
            settled = new bool[size];
            index = new int[size];

            for (int i = 0; i < weight.Length; i++) {
                weight[i] = INFINITY;
                settled[i] = false;
                index[i] = 0;
            }

            size = 0;
            maxUsedSize = 0;
            usedSize = 0;
        }

        // Recursive space complexity = o(1). Time complexity = o(n) worse case where n is size of array. (Usually o(1).)
        public override bool insert(int pointer, double value) {
            int x = 0;
            // First try to add to end
            for (x = usedEver; x < index.Length; x++) {
                if (weight[x] == INFINITY) {
                    index[x] = pointer;
                    weight[x] = value;
                    settled[x] = false;
                    usedEver++;
                    usedSize++;
                    if (usedSize > maxUsedSize) {
                        maxUsedSize++;
                    }
                    return true;
                }
            }
            // If you can't do that, walk the list
            for (x = 0; x < index.Length; x++) {
                if (settled[x] == true) {
                    index[x] = pointer;
                    weight[x] = value;
                    settled[x] = false;
                    usedSize++;
                    if (usedSize > maxUsedSize) {
                        maxUsedSize++;
                    }
                    return true;
                }
            }
            return false;
        }

        // Decrease the distance. Recursive space complexity = o(1). Time complexity = o(1)
        public override void decreasekey(int index, float value) {
            weight[index] = value;
        }

        // Pop off the next minumum value. Recursive space complexity = o(1). Time complexity = o(n) where n is size of array.
        public override int deletemin() {
            usedSize--;

            // Note so we have to walk through the list of weights and find the smallest
            double least = INFINITY;
            int position = 0;
            for (int i = 0; i < usedEver + 1; i++) {
                if (settled[i] == true) {
                    continue;
                }
                if (weight[i] < least) {
                    least = weight[i];
                    position = i;
                }
            }

            settled[position] = true;
            return index[position];
        }
    }
}

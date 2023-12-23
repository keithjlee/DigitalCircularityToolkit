using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.LinearSolver;

namespace DigitalCircularityToolkit.Assignment
{
    public static class LSAP
    {
        /// <summary>
        /// Primarily derived from https://developers.google.com/optimization/assignment/assignment_example
        /// </summary>
        /// <param name="cost_matrix"></param>
        /// <returns></returns>
        public static int[] Assign_SCIP(int[,] costs, out double total_cost)
        {
            int numWorkers = costs.GetLength(0);
            int numTasks = costs.GetLength(1);

            // Solver.
            Solver solver = Solver.CreateSolver("SCIP");
            if (solver is null)
            {
                total_cost = -1;
                return new int[0];
            }

            // Variables.
            // x[i, j] is an array of 0-1 variables, which will be 1
            // if worker i is assigned to task j.
            Variable[,] x = new Variable[numWorkers, numTasks];
            for (int i = 0; i < numWorkers; ++i)
            {
                for (int j = 0; j < numTasks; ++j)
                {
                    x[i, j] = solver.MakeIntVar(0, 1, $"supply_{i}_demand_{j}");
                }
            }

            // Each task is assigned to exactly one worker.
            // UPDATE THIS TO REFLECT DUPLICATED GEOMETRIES EFFECTIVELY
            for (int j = 0; j < numTasks; ++j)
            {
                Constraint constraint = solver.MakeConstraint(1, 1, "");
                for (int i = 0; i < numWorkers; ++i)
                {
                    constraint.SetCoefficient(x[i, j], 1);
                }
            }

            // Objective
            Objective objective = solver.Objective();
            for (int i = 0; i < numWorkers; ++i)
            {
                for (int j = 0; j < numTasks; ++j)
                {
                    objective.SetCoefficient(x[i, j], costs[i, j]);
                }
            }
            objective.SetMinimization();

            // Solve
            Solver.ResultStatus resultStatus = solver.Solve();

            // Get total cost
            total_cost = solver.Objective().Value();

            // Get assignment vector
            int[] assignments = new int[numTasks];

            for (int i = 0; i < numWorkers; i++)
            {
                for (int j = 0; j < numTasks; j++)
                {
                    if (x[i,j].SolutionValue() > 0.5)
                    {
                        assignments[j] = i;
                    }
                }
            }

            return assignments;
        }
    }
}

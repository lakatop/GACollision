from collections import namedtuple
import os
import glob
import warnings

import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

"""
Given multiple files in csv format as following:
CROSS,BasicCrossOperatorParallel
MUTATION,BasicMutationOperatorParallel
FITNESS,BasicFitnessFunctionParallel
SELECTION,BasicSelectionFunctionParallel
INITIALIZATION,GlobeInitialization

Fitness,Objective
0.04444315,1.859636
0.1946334,2.314323
0.0594056,1.293208
0.06728838,1.433224
0.07287577,1.409458
0.06531519,1.967832
0.09004448,1.680128
0.119467,1.680128
0.1361938,1.680128
0.1333348,1.680128
0.2012125,1.680128

Load each file into dataframe and plot the fitness and objective values
All plots are then saved into a file
"""

path = "../straightLine/*.csv"

# Get a list of all CSV files in the current directory
csv_files = sorted(
    glob.glob(path), key=lambda x: int(x.lstrip("../straightLine/out").rstrip(".csv"))
)


# Create subplots
fig, axes = plt.subplots(
    nrows=len(csv_files) * 2, ncols=1, figsize=(10, 6 * len(csv_files))
)

result = ""
with open("../straightLine/out0.csv", "r") as input_file:
    # files are iterable, you can have a for-loop over a file.
    for line_number, line in enumerate(input_file):
        if line_number > 5:  # line_number starts at 0.
            break
        result += line
        result += "\n"


fig.suptitle(result, fontsize=16)

# Iterate over each CSV file
for idx, csv_file in enumerate(csv_files):
    # Read CSV file into a DataFrame
    df = pd.read_csv(csv_file, skiprows=7)

    # Add a new column for line number (iteration)
    df["iteration"] = range(1, len(df) + 1)

    # Plot each column in the DataFrame on a separate subplot
    it = 0
    for column in df.columns:
        print(column)
        if column == "iteration":
            continue
        label = ""
        if it % 2 == 0:
            label = "Fitness Values"
            axes[idx * 2].plot(
                df["iteration"], df[column], marker="o", label=label, color="tab:blue"
            )
        elif it % 2 == 1:
            label = "Objective Values"
            axes[idx * 2 + 1].plot(
                df["iteration"], df[column], marker="o", label=label, color="tab:red"
            )

        it += 1

    # Add labels and legend to the subplot
    axes[idx * 2].set_title(f"{csv_file} - Data over Iterations")
    axes[idx * 2].set_xlabel("Iteration")
    axes[idx * 2].set_ylabel("Values")
    axes[idx * 2].legend()

    axes[idx * 2 + 1].set_title(f"{csv_file} - Data over Iterations")
    axes[idx * 2 + 1].set_xlabel("Iteration")
    axes[idx * 2 + 1].set_ylabel("Values")
    axes[idx * 2 + 1].legend()


# Adjust layout and save the figure
plt.tight_layout()
fig.subplots_adjust(top=0.95)
plt.savefig("output_graphs.png")
plt.show()

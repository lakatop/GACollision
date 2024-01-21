from collections import namedtuple
import os
import glob
import warnings
import re

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

Fitness
0.04444315
0.1946334
0.0594056
0.06728838
0.07287577
0.06531519
0.09004448
0.119467
0.1361938
0.1333348
0.2012125

Load each file into dataframe and plot the fitness and objective values
All plots are then saved into a file
"""

directories = ["straightLine", "smallObstacle", "cornerProblem", "oppositeAgents"]

for directory in directories:
    path = directory + "/*.csv"

    # Get a list of all CSV files in the current directory
    # csv_files = glob.glob(path)
    csv_files = sorted(
        glob.glob(path),
        key=lambda x: int(re.sub(directory + r"/out-[0-9]+-", "", x.rstrip(".csv")))
        #  int(
        #     x.replace(directory + "/out-", "").replace(r"-[0-9]+.csv", "")
        # ),
    )

    # Create subplots
    fig, axes = plt.subplots(
        nrows=len(csv_files), ncols=1, figsize=(10, 6 * len(csv_files))
    )

    result = ""
    with open("straightLine/out-1-0.csv", "r") as input_file:
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
            label = "Fitness Values"
            axes[idx].plot(
                df["iteration"], df[column], marker="o", label=label, color="tab:blue"
            )

            it += 1

        # Add labels and legend to the subplot
        axes[idx].set_title(f"{csv_file} - Data over Iterations")
        axes[idx].set_xlabel("Iteration")
        axes[idx].set_ylabel("Values")
        axes[idx].legend()

    # Adjust layout and save the figure
    # plt.tight_layout()
    # fig.subplots_adjust(top=0.99)
    pngName = directory + ".png"
    plt.savefig(pngName)
    # plt.show()

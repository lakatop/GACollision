from collections import namedtuple
import os

import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

"""
Given multiple files in csv format as following:
PathLength,PathDuration,CollisionCount,FramesInCollision,PathJerk,GaTimes
40,30.8306323749712,0,0,0.9040295,53.1863270443864
...

Creates average of all the files and saves it into a new file (for each row separately)
"""
main_directory = "Runs"

directories = [
    "straightLine",
    "smallObstacle",
    "oppositeMultipleAgents",
    "oppositeCircleAgents",
    "oppositeAgents",
    "narrowCoridorsOppositeNoNavmeshScenario",
    "narrowCoridorOpposite",
    "cornerSingle",
]

runs = [
    name
    for name in os.listdir(main_directory)
    if os.path.isdir(os.path.join(main_directory, name))
]

for run in runs:
    print("dirs", run)
    print("dir", dir)
    # Create a new directory for the averages
    new_directory = main_directory + "/" + run + "/" + "Averages"
    print(new_directory)
    if not os.path.exists(new_directory):
        os.makedirs(new_directory)

    for directory in directories:
        path = main_directory + "/" + run + "/" + directory

        # Get list of CSV files in the directory
        csv_files = [
            os.path.join(path, file)
            for file in os.listdir(path)
            if file.endswith(".csv")
        ]

        if not csv_files:
            print("No CSV files found in the directory.")
            exit()

        # Read CSV files into a list of DataFrames
        dfs = [pd.read_csv(file) for file in csv_files]

        # Compute the average of each row across all DataFrames
        average_df = sum(dfs) / len(dfs)
        average_df["iteration"] = range(0, len(average_df))
        average_df["run_id"] = run

        # Concatenate all DataFrames
        # concatenated_df = pd.concat(average_df, ignore_index=True)

        # Write the averages to a new CSV file
        output_file = (
            main_directory + "/" + run + "/Averages/" + directory + "output.csv"
        )
        average_df.to_csv(output_file, index=False)
        print("Data written to", output_file)

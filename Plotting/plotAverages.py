from collections import namedtuple
import os

import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

main_directory = "Runs"

runs = [
    name
    for name in os.listdir(main_directory)
    if os.path.isdir(os.path.join(main_directory, name))
]

for run in runs:
    # Get the absolute path of the directory
    directory_path = os.path.abspath(main_directory + "/" + run + "/Averages")

    # Get list of CSV files in the directory
    csv_files = [
        os.path.join(directory_path, file)
        for file in os.listdir(directory_path)
        if file.endswith(".csv")
    ]

    if not csv_files:
        print("No CSV files found in the directory.")
        exit()

    # Create graphs for each run separately
    for file in csv_files:
        df = pd.read_csv(file)

        # Get column names (except 'iteration')
        column_names = [
            col for col in df.columns if col != "iteration" and col != "run_id"
        ]

        # Plot each column in separate subplot
        num_subplots = len(column_names)
        fig, axes = plt.subplots(num_subplots, 1, figsize=(10, 6 * num_subplots))

        for i, col in enumerate(column_names):
            ax = axes[i]
            ax.plot(df["iteration"], df[col])
            ax.set_xlabel("Iteration")
            ax.set_ylabel(col)
            ax.set_title(col)
            ax.set_xticks(range(len(df["iteration"])))  # Set x-axis ticks as integers
            ax.grid(True)  # Add grid lines for better readability

        plt.tight_layout()
        # plt.show()
        plt.savefig(file.replace(".csv", ".png"))

# Create a single graph for all runs
# Read all CSV files into a list of DataFrames
directory_path = os.path.abspath(main_directory + "/" + runs[0] + "/RunAverage")
csv_files = [
    os.path.join(directory_path, file)
    for file in os.listdir(directory_path)
    if file.endswith(".csv")
]
for file in csv_files:
    df = []
    for run in runs:
        df.append(pd.read_csv(file))

    column_names = [
        col for col in df[0].columns if col != "iteration" and col != "run_id"
    ]

    num_subplots = len(column_names)
    fig, axes = plt.subplots(num_subplots, 1, figsize=(10, 6 * num_subplots))

    # Plot each column in separate subplot
    # X axis will be run_id
    for i, col in enumerate(column_names):
        ax = axes[i]
        for j, run in enumerate(runs):
            ax.plot(df[j]["run_id"], df[j][col], label=run)
        ax.set_xlabel("Run ID")
        ax.set_ylabel(col)
        ax.set_title(col)
        # ax.set_xticks(range(len(runs)))  # Set x-axis ticks as integers
        ax.grid(True)  # Add grid lines for better readability
        ax.legend()

    plt.tight_layout()
    plt.savefig(
        main_directory + "/all_runs_" + file.split("/")[-1].replace(".csv", ".png")
    )
    # plt.show()

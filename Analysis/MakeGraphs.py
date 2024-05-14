import math
import sys
from scipy import stats as st
import numpy as np
import matplotlib.pyplot as plot
import argparse
import os

def ReadData(path):
    with open(path,'r') as file:
        data = np.loadtxt(file, delimiter=";", skiprows=1)
        data = data.transpose()
        return data

parser = argparse.ArgumentParser()
parser.add_argument("file", nargs=2, help='<Required> Provide inputs, exactly 2, same replay, same amount of iterations, name format REPLAY_ID_DATETIME_ITERATIONS.data')

class Empty:
    pass
def parse(path): 
    result = Empty()
    result.path = path
    result.replay, result.paradigm, result.date, result.iterations = os.path.splitext(os.path.basename(os.path.normpath(path)))[0].split('_')
    result.cacherefs, result.cachemisses, result.energyuse, result.time = ReadData(path)
    return result

figsize = (15, 12)

results = list(map(parse, parser.parse_args().file))

plotnames = ['cacheref_misses_scatter', 'energy_boxplot', 'time_energy_scatter', 'time_boxplot']

replay = results[0].replay
iterations = results[0].iterations
for i, result in enumerate(results):
    if (replay != result.replay): sys.exit("Not the same replay")
    if (iterations != result.iterations): sys.exit("Not the same amount of iterations")
    
    plot.figure(0, figsize=figsize)
    plot.scatter(x = result.cacherefs, y = result.cachemisses, label = result.paradigm)
    plot.xlabel("Cache refs")
    plot.ylabel("Cache misses")
    plot.legend(loc = "upper left")
    
    plot.figure(1, figsize=figsize)
    plot.boxplot(x = result.energyuse, positions = [i], labels = [result.paradigm])
    plot.ylabel("Energy (J)")

    plot.figure(2, figsize=figsize)
    plot.scatter(x = result.time, y = result.energyuse, label = result.paradigm)
    plot.xlabel("Time (s)")
    plot.ylabel("Energy use (J)")
    plot.legend(loc = "upper left")

    plot.figure(3, figsize=figsize)
    plot.boxplot(x = result.time, positions = [i], labels = [result.paradigm])
    plot.ylabel("Time (s)")

plots_path = os.path.join('..','Plots')
if not os.path.exists(plots_path):
    os.makedirs(plots_path)

for i in range(0,4):
    plot.figure(i)
    plot.savefig(os.path.join(plots_path,f'{replay}-{plotnames[i]}.png'))
    


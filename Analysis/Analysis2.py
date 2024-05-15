import math
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
parser.add_argument("files", nargs="+", help='<Required> Provide inputs')

class Empty:
    pass
def parse(path): 
    result = Empty()
    result.path = path
    result.replay, result.paradigm, result.date, result.iterations = os.path.splitext(os.path.basename(os.path.normpath(path)))[0].split('_')
    result.cacherefs, result.cachemisses, result.energyuse, result.time = ReadData(path) #! make sure this matches the layout of the file
    return result

fig, ax = plot.subplots(2,2)

results = list(map(parse, parser.parse_args().files))

for i, result in enumerate(results):
    ax[0,0].scatter(x = result.cacherefs, y = result.cachemisses, label = result.paradigm)
    ax[0,1].boxplot(x = result.energyuse, positions = [i], labels = [result.paradigm])
    ax[1,0].scatter(x = result.time, y = result.energyuse, label = result.paradigm)
    ax[1,1].boxplot(x = result.time, positions = [i], labels = [result.paradigm])

ax[0,0].set_xlabel("Cache refs")
ax[0,0].set_ylabel("Cache misses")
ax[0,0].legend()

ax[0,1].set_ylabel("Energy (J)")

ax[1,0].set_xlabel("Time (s)")
ax[1,0].set_ylabel("Energy use (J)")
ax[1,0].legend()

ax[1,1].set_ylabel("Time (s)")



plot.show()

# OOPcacherefsSTD = np.std(OOPcacherefs)
# OOPcacherefsVar = np.var(OOPcacherefs)
# OOPcachemissesSTD = np.std(OOPcachemisses)
# OOPcachemissesVar = np.var(OOPcachemisses)
# OOPenergyuseSTD = np.std(OOPenergyuse)
# OOPenergyuseVar = np.var(OOPenergyuse)

# DOPcacherefsSTD = np.std(DOPcacherefs)
# DOPcacherefsVar = np.var(DOPcacherefs)
# DOPcachemissesSTD = np.std(DOPcachemisses)
# DOPcachemissesVar = np.var(DOPcachemisses)
# DOPenergyuseSTD = np.std(DOPenergyuse)
# DOPenergyuseVar = np.var(DOPenergyuse)

# def printStatistics(stddev, variance, name):
#     print(name + " standard deviation: " + np.float64(stddev).astype(str) + "; variance: " +np.float64(variance).astype(str))

# printStatistics(OOPcacherefsSTD, OOPcacherefsVar, "OOP cache references")
# printStatistics(OOPcachemissesSTD, OOPcachemissesVar, "OOP cache misses")
# printStatistics(OOPenergyuseSTD, OOPenergyuseVar, "OOP energy use")

# printStatistics(DOPcacherefsSTD, DOPcacherefsVar, "DOP cache references")
# printStatistics(DOPcachemissesSTD, DOPcachemissesVar, "DOP cache misses")
# printStatistics(DOPenergyuseSTD, DOPenergyuseVar, "DOP energy use")

def ConfidenceInterval(data, name, confidence = 0.95):
    mean = np.mean(data)
    sampleSize = len(data)
    stddev = np.std(data)
    interval = confidence*(stddev / math.sqrt(sampleSize))
    print("Confidence interval for " +name+" is: " + np.float64(mean).astype(str) + "+-" + np.float64(interval).astype(str))    

ConfidenceInterval(OOPcacherefs, "OOP cache references")
ConfidenceInterval(OOPcachemisses, "OOP cache misses")
ConfidenceInterval(OOPenergyuse, "OOP energy use")
ConfidenceInterval(DOPcacherefs, "DOP cache references")
ConfidenceInterval(DOPcachemisses, "DOP cache misses")
ConfidenceInterval(DOPenergyuse, "DOP energy use")


CacherefTTest = st.ttest_ind(OOPcacherefs, DOPcacherefs, equal_var=False)
print("The P value for Cache refs is: " + np.float64(CacherefTTest.pvalue).astype(str))

CachemissTTest = st.ttest_ind(OOPcachemisses, DOPcachemisses, equal_var=False)
print("The P value for Cache misses is: " + np.float64(CachemissTTest.pvalue).astype(str))

energyTTest = st.ttest_ind(OOPenergyuse, DOPenergyuse, equal_var=False)
print("The P value for energy is: " + np.float64(energyTTest.pvalue).astype(str))

OOPTimeEnergyR = st.pearsonr(OOPtime, OOPenergyuse)
print("The R value for time and energy in OOP is: " + np.float64(OOPTimeEnergyR.statistic).astype(str))

DOPTimeEnergyR = st.pearsonr(DOPtime, DOPenergyuse)
print("The R value for time and energy in DOP is: " + np.float64(DOPTimeEnergyR.statistic).astype(str))

bothTime = np.concatenate((OOPtime, DOPtime))
bothEnergy = np.concatenate((OOPenergyuse, DOPenergyuse))

bothTimeEnergyR = st.pearsonr(bothTime, bothEnergy)
print("The R value for time and energy for both versions is: " + np.float64(bothTimeEnergyR.statistic).astype(str))
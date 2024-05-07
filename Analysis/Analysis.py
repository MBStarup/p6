import math
from scipy import stats as st
import numpy as np
import matplotlib.pyplot as plot

with open('../Measurements/OOPOnlyMove','r') as OOPfile:
    OOPdata = np.loadtxt(OOPfile, delimiter=";", skiprows=1)
    OOPdata = OOPdata.transpose()
    OOPcacherefs = OOPdata[0]
    OOPcachemisses = OOPdata[1]
    OOPenergyuse = OOPdata[2]
    OOPtime = OOPdata[3]
with open("../Measurements/DOPOnlyMove","r") as DOPfile:
    DOPdata = np.loadtxt(DOPfile, delimiter=";", skiprows=1,)
    DOPdata = DOPdata.transpose()
    DOPcacherefs = DOPdata[0]
    DOPcachemisses = DOPdata[1]
    DOPenergyuse = DOPdata[2]
    DOPtime = DOPdata[3]
    
fig, ax = plot.subplots()
ax.scatter(OOPcacherefs, OOPcachemisses, c="red")
ax.scatter(DOPcacherefs, DOPcachemisses, c="blue")
ax.set_xlabel("Chache references")
ax.set_ylabel("Cache misses")
ax.set_aspect("equal")
plot.show()

fig, ax = plot.subplots()
ax.scatter(OOPtime, OOPenergyuse , c="red")
ax.scatter(DOPtime, DOPenergyuse, c="blue")
ax.set_xlabel("Time (s)")
ax.set_ylabel("Energy use (J)")
plot.show()

energyData = (OOPenergyuse, DOPenergyuse)
plot.boxplot(energyData, labels=["OOP", "DOP"])
plot.ylabel("Energy (J)")
plot.show()

timedata = (OOPtime, DOPtime)
plot.boxplot(timedata, labels=["OOP", "DOP"])
plot.ylabel("Time (s)")
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
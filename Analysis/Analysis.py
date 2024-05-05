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

energyData = (OOPenergyuse, DOPenergyuse)
plot.boxplot(energyData, labels=["OOP", "DOP"])
plot.ylabel("Energy (J)")
plot.show()

timedata = (OOPtime, DOPtime)
plot.boxplot(timedata, labels=["OOP", "DOP"])
plot.ylabel("Time (s)")
plot.show()
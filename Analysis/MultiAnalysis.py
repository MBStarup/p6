import math
from scipy import stats as st
import numpy as np
import matplotlib.pyplot as plot

def ReadData(path):
    with open(path,'r') as file:
        data = np.loadtxt(file, delimiter=";", skiprows=1)
        data = data.transpose()
        return data


OOPdata1 = ReadData('../Measurements/OOPOnlyMove1')
OOPdata2 = ReadData('../Measurements/OOPOnlyMove2')
OOPdata3 = ReadData('../Measurements/OOPOnlyMove3')
DOPdata1 = ReadData('../Measurements/DOPOnlyMove1')
DOPdata2 = ReadData('../Measurements/DOPOnlyMove2')
    
fig, axs = plot.subplots(3,2)
axs[0, 0].scatter(OOPdata1[0], OOPdata1[1], c="red")
axs[0, 0].set_title("OOP 1")
axs[1, 0].scatter(OOPdata2[0], OOPdata2[1], c="red")
axs[1, 0].set_title("OOP 2")
axs[2, 0].scatter(OOPdata3[0], OOPdata3[1], c="red")
axs[2, 0].set_title("OOP 3")
axs[1, 1].scatter(DOPdata1[0], DOPdata1[1], c="blue")
axs[1, 1].set_title("DOP 1")
axs[2, 1].scatter(DOPdata2[0], DOPdata2[1], c="blue")
axs[2, 1].set_title("DOP 2")
fig.delaxes(axs[0,1])

for ax in axs.flat:
    ax.set(xlabel = "Chache references", ylabel =  "Cache misses")
    # ax.set_aspect("equal")
for ax in fig.get_axes():
    ax.label_outer()
plot.show()

fig, axs = plot.subplots(3,2)
axs[0, 0].scatter(OOPdata1[3], OOPdata1[2], c="red")
axs[0, 0].set_title("OOP 1")
axs[1, 0].scatter(OOPdata2[3], OOPdata2[2], c="red")
axs[1, 0].set_title("OOP 2")
axs[2, 0].scatter(OOPdata3[3], OOPdata3[2], c="red")
axs[2, 0].set_title("OOP 3")
axs[1, 1].scatter(DOPdata1[3], DOPdata1[2], c="blue")
axs[1, 1].set_title("DOP 1")
axs[2, 1].scatter(DOPdata2[3], DOPdata2[2], c="blue")
axs[2, 1].set_title("DOP 2")
fig.delaxes(axs[0,1])

for ax in axs.flat:
    ax.set(xlabel = "Time (s)", ylabel =  "Energy (J)")
    # ax.set_aspect("equal")
for ax in fig.get_axes():
    ax.label_outer()
plot.show()

energyData = (OOPdata1[2], OOPdata2[2], OOPdata3[2], DOPdata1[2], DOPdata2[2])
plot.boxplot(energyData, labels=["OOP1","OOP2","OOP3", "DOP1", "DOP2"])
plot.ylabel("Energy (J)")
plot.show()

timedata = (OOPdata1[3], OOPdata2[3], OOPdata3[3], DOPdata1[3], DOPdata2[3])
plot.boxplot(timedata, labels=["OOP1","OOP2","OOP3", "DOP1", "DOP2"])
plot.ylabel("Time (s)")
plot.show()
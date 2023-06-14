import numpy as np
import pandas as pd
# from sklearn.ensemble import RandomForestRegressor
# from sklearn.linear_model import LinearRegression
# from matplotlib import pyplot
from joblib import dump, load
import sys
import os

def main():
    # print("Number of Args: %s" % len(sys.argv))
    # print("Argv: %s" % str(sys.argv))
    xrating_model = load(sys.argv[-1])
    features = np.array(sys.argv[1:-1]).reshape(1, -1)
    names = ["shots", "goals", "assists", "saves", "passes", 
             "tackles", "interceptions", "dribbles", "fouls", "yellows", "reds"]
    print(xrating_model.predict(pd.DataFrame(features, columns=names))[0])

if __name__ == "__main__": main()
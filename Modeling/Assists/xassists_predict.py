import numpy as np
import pandas as pd
# from sklearn.ensemble import RandomForestRegressor
# from sklearn.linear_model import LinearRegression
# from matplotlib import pyplot
from sklearn.preprocessing import PolynomialFeatures
from joblib import dump, load
import sys
import os

def bound(a): return 0 if a < 0 else a

def main():
    # print("Number of Args: %s" % len(sys.argv))
    # print("Argv: %s" % str(sys.argv))
    xassists_model = load(sys.argv[-1])
    features = pd.DataFrame(np.array(sys.argv[1:-1]).reshape(1, -1), 
                            columns=["avgPasses","avgKeyPasses","avgPass_Percentage","avgAssists","avgDribbles","avgDribblesWon",
                                     "avgMinutes","Percent_Assisted","avgTeamScored","avgOppConceded","avgTeamShots","avgTeamShotsOG","avgPossesion",
                                     "height","width"])
    print(bound(xassists_model.predict(PolynomialFeatures(degree=3, include_bias=False).fit_transform(features))[0]))

if __name__ == "__main__": main()